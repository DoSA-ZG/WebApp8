using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;


namespace RPPP_WebApp.Controllers
{
	/// <summary>
	/// Kontroler za sve akcije vezane uz zadatke
	/// </summary>
    public class ZadatakController : Controller
    {

        private readonly Rppp08Context ctx;
		private readonly ILogger<ZahtjevController> logger;
		private readonly AppSettings appSettings;

        /// <summary>
        /// Inicijalizira novu instancu klase <see cref="ZadatakController"/>.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Postavke aplikacije.</param>
        /// <param name="logger">Logger za zapisivanje poruka.</param>
        public ZadatakController(Rppp08Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevController> logger)
        {
            this.ctx = ctx;
			this.logger = logger;
			appSettings = options.Value;
		}

        /// <summary>
        /// Prikazuje listu Zadataka sa opcijom sortiranja i filtriranja.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Opcija sortiranja.</param>
        /// <param name="ascending">Redoslijed sortiranja.</param>
        /// <returns>Pregled s paginiranom listom Zadataka.</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
		{
			int pageSize = appSettings.PageSize;

			var query = ctx.Zadataks
						   .AsNoTracking();

			int count = query.Count();

			if (count == 0)
			{
				logger.LogInformation("Ne postoji nijedan zadatak.");
				TempData[Constants.Message] = "Ne postoji niti jedan zadatak.";
				TempData[Constants.ErrorOccurred] = false;
				return RedirectToAction(nameof(Create));
			}

			var pagingInfo = new PagingInfo
			{
				CurrentPage = page,
				Sort = sort,
				Ascending = ascending,
				ItemsPerPage = pageSize,
				TotalItems = count
			};

			if (page < 1 || page > pagingInfo.TotalPages)
			{
				return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
			}

			query = query.ApplySort(sort, ascending);

			var zadatci = query
						   .Select(z => new ZadatakViewModel
						   {
							   ZadatakId = z.ZadatakId,
							   Opis = z.Opis,
							   StatusZadatka = z.StatusZadatka.NazivStatusaZadatka,
							   OznakaZahtjeva = z.Zahtjev.Oznaka
						   })
						   .Skip((page - 1) * pageSize)
						   .Take(pageSize)
						   .ToList();

			var model = new ZadatciViewModel
			{
				Zadatci = zadatci,
				PagingInfo = pagingInfo
			};

			return View(model);
		}


        /// <summary>
        /// Priprema padajuće liste potrebne za stvaranje novog Zadatka.
        /// </summary>
        private async Task PrepareDropDownList()
		{
			var StatusZadatka = await ctx.StatusZadatkas
									.Select(sz => new { sz.StatusZadatkaId, sz.NazivStatusaZadatka })
									.ToListAsync();

			var Zahtjevi = await ctx.Zahtjevs
									.Select(sz => new { sz.ZahtjevId, sz.Oznaka})
									.ToListAsync();

			ViewBag.StatusZadatka = new SelectList(StatusZadatka, "StatusZadatkaId", "NazivStatusaZadatka");
			ViewBag.Zahtjevi = new SelectList(Zahtjevi, "ZahtjevId", "Oznaka");
		}


        /// <summary>
        /// Prikazuje formu za stvaranje novog Zahtjeva.
        /// </summary>
        /// <returns>View za stvaranje novog Zahtjeva.</returns>
        [HttpGet]
		public async Task<IActionResult> Create()
		{
			await PrepareDropDownList();
			return View();
		}



        /// <summary>
        /// Sprema novi Zadatak u bazu podataka.
        /// </summary>
        /// <param name="zadatak">Podaci o novom Zadatku.</param>
        /// <returns>Preusmjerava na pregled svih Zadataka nakon spremanja.</returns>
        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Zadatak zadatak)
		{
			if (ModelState.IsValid)
			{
				try
				{
					ctx.Add(zadatak);
					await ctx.SaveChangesAsync();

					TempData[Constants.Message] = $"Zadatak {zadatak.ZadatakId} dodan.";
					TempData[Constants.ErrorOccurred] = false;
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
					await PrepareDropDownList();
					return View(zadatak);
				}
			}
			await PrepareDropDownList();
			return View(zadatak);
		}


        /// <summary>
        /// Briše Zadatak iz baze podataka.
        /// </summary>
        /// <param name="id">Identifikator Zadatka koji treba biti obrisan.</param>
        /// <returns>Preusmjerava na pregled svih Zadataka nakon brisanja.</returns>
        [HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var zadatak = await ctx.Zadataks
							 .FindAsync(id);

			try
			{
				ctx.Remove(zadatak);
				await ctx.SaveChangesAsync();
				TempData[Constants.Message] = $"Zadatak {zadatak.ZadatakId} obrisan.";
				TempData[Constants.ErrorOccurred] = false;
			}
			catch (Exception ex)
			{	
				
				TempData[Constants.Message] = $"Neuspijeh pri brisanju zadatka! ${ex.Message}" ;
				TempData[Constants.ErrorOccurred] = true;
			}

			return RedirectToAction("Index");
		}


        /// <summary>
        /// Prikazuje formu za uređivanje Zadatka prema zadatom identifikatoru.
        /// </summary>
        /// <param name="id">Identifikator Zadatka koji se uređuje.</param>
        /// <returns>View za uređivanje Zadatka.</returns>
        [HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var zadatak = ctx.Zadataks.Find(id);
			if (zadatak == null)
			{
				return NotFound("Ne postoji kartica s id : " + id);
			}
			await PrepareDropDownList();
			return View(zadatak);
		}


        /// <summary>
        /// Ažurira informacije o Zadatku u bazi podataka.
        /// </summary>
        /// <param name="id">Identifikator Zadatka koji se ažurira.</param>
        /// <returns>Preusmjerava na pregled svih Zadataka nakon ažuriranja.</returns>
        [HttpPost, ActionName("Edit")]
		public async Task<IActionResult> Update(int id)
		{
			var zadatak = await ctx.Zadataks
									.Where(z => z.ZadatakId == id)
									.FirstOrDefaultAsync();

			if (zadatak == null)
			{
				return NotFound("Ne postoji zahtjev s id : " + id);
			}

			if (await TryUpdateModelAsync<Zadatak>(zadatak, "", z => z.Opis, z => z.StatusZadatkaId, z => z.ZahtjevId))
			{
				try
				{
					await ctx.SaveChangesAsync();
					TempData[Constants.Message] = $"Zahtjev {zadatak.ZadatakId} uspješno ažuriran.";
					TempData[Constants.ErrorOccurred] = false;
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Neuspijeh pri spremanju podataka.");
				}
			}

			await PrepareDropDownList();
			return View(zadatak);
		}
	}
}

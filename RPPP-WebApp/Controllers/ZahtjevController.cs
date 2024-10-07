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
    /// Kontroler za sve akcije vezane uz zahtjeve (DZ1)
    /// </summary>
    public class ZahtjevController : Controller
	{
		private readonly Rppp08Context ctx;
		private readonly ILogger<ZahtjevController> logger;
		private readonly AppSettings appSettings;

        /// <summary>
        /// Inicijalizira novu instancu klase <see cref="ZahtjevController"/>.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Postavke aplikacije.</param>
        /// <param name="logger">Logger za zapisivanje poruka.</param>
        public ZahtjevController(Rppp08Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevController> logger)
		{
			this.ctx = ctx;
			this.logger = logger;
			appSettings = options.Value;
		}


        /// <summary>
        /// Prikazuje listu Zahtjeva sa opcijom sortiranja i filtriranja.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Opcija sortiranja.</param>
        /// <param name="ascending">Redoslijed sortiranja.</param>
        /// <returns>Pregled s paginiranom listom Zahtjeva.</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
		{
			int pageSize = appSettings.PageSize;

			var query = ctx.Zahtjevs
						   .AsNoTracking();

			int count = query.Count();

			if (count == 0)
			{
				logger.LogInformation("Ne postoji nijedan zahtjev");
				TempData[Constants.Message] = "Ne postoji niti jedan zahtjev.";
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
				return RedirectToAction(nameof(Index), new {page = 1, sort, ascending});
			}

			query = query.ApplySort(sort, ascending);

			var zahtjevi = query
						   .Select(z => new ZahtjevViewModel
						   {
							   ZahtjevId = z.ZahtjevId,
							   Oznaka = z.Oznaka,
							   NazivZahtjeva = z.NazivZahtjeva,
							   NazivVrsteZahtjeva = z.VrstaZahtjeva.NazivVrsteZahtjeva,
							   NazivProjekta = z.Projekt.NazivProjekta,
							   Prioritet = z.Prioritet,
							   Zadatci = z.Zadataks
						   })
						   .Skip((page - 1) * pageSize)
						   .Take(pageSize)
						   .ToList();

			var model = new ZahtjeviViewModel
			{
				Zahtjevi = zahtjevi,
				PagingInfo = pagingInfo
			};

			return View(model);
		}

        /// <summary>
        /// Priprema padajuće liste potrebne za stvaranje novog Zadatka.
        /// </summary>
        private async Task PrepareDropDownList()
		{
			var VrstaZahtjeva = await ctx.VrstaZahtjevas
									.Select(vz => new { vz.VrstaZahtjevaId, vz.NazivVrsteZahtjeva })
									.ToListAsync();
			var Projekti = await ctx.Projekts
									.Select(p => new { p.ProjektId, p.NazivProjekta})
									.ToListAsync();

			ViewBag.VrsteZahtjeva = new SelectList(VrstaZahtjeva, "VrstaZahtjevaId", "NazivVrsteZahtjeva");
			ViewBag.Projekti = new SelectList(Projekti, "ProjektId", "NazivProjekta");
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
        /// Sprema novi Zahtjev u bazu podataka.
        /// </summary>
        /// <param name="zahtjev">Podaci o novom Zahtjevu.</param>
        /// <returns>Preusmjerava na pregled svih Zahtjeva nakon spremanja.</returns>
        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Zahtjev zahtjev)
		{
			if (ModelState.IsValid)
			{
				try
				{
					ctx.Add(zahtjev);
					await ctx.SaveChangesAsync();

					TempData[Constants.Message] = $"Zahtjev {zahtjev.Oznaka} dodan.";
					TempData[Constants.ErrorOccurred] = false;
					return RedirectToAction("Index");
				}
				catch(Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
					await PrepareDropDownList();
					return View(zahtjev);
				}
			}
			await PrepareDropDownList();
			return View(zahtjev);
		}


        /// <summary>
        /// Briše Zahtjev iz baze podataka.
        /// </summary>
        /// <param name="id">Identifikator Zahtjeva koji treba biti obrisan.</param>
        /// <returns>Preusmjerava na pregled svih Zahtjeva nakon brisanja.</returns>
        [HttpPost]
		public async Task<IActionResult> Delete(int ZahtjevId)
		{
			var zahtjev = await ctx.Zahtjevs
							 .FindAsync(ZahtjevId);

			Console.WriteLine(zahtjev);

			try
			{
				ctx.Remove(zahtjev);
				await ctx.SaveChangesAsync();
				TempData[Constants.Message] = $"Zahtjev {zahtjev.Oznaka} obrisan.";
				TempData[Constants.ErrorOccurred] = false;
			}
			catch(Exception ex) {
				TempData[Constants.Message] = $"Neuspijeh pri brisanju zahtjeva!";
				TempData[Constants.ErrorOccurred] = true;
			}

			return RedirectToAction("Index");
		}


        /// <summary>
        /// Prikazuje formu za uređivanje Zahtjeva prema zadatom identifikatoru.
        /// </summary>
        /// <param name="id">Identifikator Zahtjeva koji se uređuje.</param>
        /// <returns>View za uređivanje Zahtjeva.</returns>
        [HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var zahtjev = ctx.Zahtjevs.Find(id);
			if (zahtjev == null)
			{
				return NotFound("Ne postoji kartica s id : " + id);
			}
			await PrepareDropDownList();
			return View(zahtjev);
		}


        /// <summary>
        /// Ažurira informacije o Zahtjevu u bazi podataka.
        /// </summary>
        /// <param name="id">Identifikator Zahtjeva koji se ažurira.</param>
        /// <returns>Preusmjerava na pregled svih Zahtjeva nakon ažuriranja.</returns>
        [HttpPost, ActionName("Edit")]
		public async Task<IActionResult> Update(int id)
		{
			var zahtjev = await ctx.Zahtjevs
									.Where(z => z.ZahtjevId == id)
									.FirstOrDefaultAsync();

			if(zahtjev == null)
			{
				return NotFound("Ne postoji zahtjev s id : " + id);
			}

			if(await TryUpdateModelAsync<Zahtjev>(zahtjev, "", z => z.Oznaka, z => z.NazivZahtjeva, z => z.VrstaZahtjevaId, z => z.ProjektId, z => z.Prioritet))
			{
				try
				{
					await ctx.SaveChangesAsync();
					TempData[Constants.Message] = $"Zahtjev {zahtjev.Oznaka} uspješno ažuriran.";
					TempData[Constants.ErrorOccurred] = false;
					return RedirectToAction("Index");
				}
				catch(Exception ex) {
					ModelState.AddModelError("", "Neuspijeh pri spremanju podataka.");
				}
			}

			await PrepareDropDownList();
			return View(zahtjev);
		}


        /// <summary>
        /// Prikazuje detalje o određenom Zahtjevu, uključujući povezane Zadatke i informacije o Statusu Zadatka.
        /// </summary>
        /// <param name="id">Identifikator Zahtjeva.</param>
        /// <returns>View sa detaljima o Zahtjevu.</returns>
        [HttpGet]
		public async Task<IActionResult> Detail(int id)
		{
			var zahtjev = await ctx.Zahtjevs
								   .Where(z => z.ZahtjevId == id)
								   .Include(z => z.Zadataks)
								   .ThenInclude(z => z.StatusZadatka)
								   .FirstOrDefaultAsync();

			if (zahtjev == null)
			{
				return NotFound("Ne postoji zahtjev s id : " + id);
			}

			await PrepareDropDownList();
			return View(zahtjev);
		}

	}
}

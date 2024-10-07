using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Models;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Extensions.Selectors;
using System.Numerics;
using RPPP_WebApp.Extensions;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za sve akcije vezane uz zahtjeve (DZ2)
    /// </summary>
	public class ZahtjevInfoController : Controller
	{

        private readonly Rppp08Context ctx;
        private readonly AppSettings appData;

        /// <summary>
        /// Inicijalizira novu instancu klase <see cref="ZahtjevInfoController"/>.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Postavke aplikacije.</param>
        public ZahtjevInfoController(Rppp08Context ctx, IOptionsSnapshot<AppSettings> options)
        {
            this.ctx = ctx;
            appData = options.Value;
        }

        /// <summary>
        /// Prikazuje paginiranu listu informacija o Zahtjevima s mogućnošću filtriranja i sortiranja.
        /// </summary>
        /// <param name="filter">Filter za pretraživanje Zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Opcija sortiranja.</param>
        /// <param name="ascending">Redoslijed sortiranja.</param>
        /// <returns>Pregled s paginiranom listom informacija o Zahtjevima.</returns>
        public async Task<IActionResult> Index(string filter, int page = 1, int sort = 1, bool ascending = true)
		{
            int pageSize = appData.PageSize;
            var query = ctx.vw_Zahtjevi.AsQueryable();

            ZahtjevFilter zf = ZahtjevFilter.FromString(filter);
            if (!zf.IsEmpty())
            {
                if (zf.ProjektId.HasValue)
                {
                    zf.NazivProjekta = await ctx.vw_Zahtjevi
                                                .Where(z => z.ProjektId == zf.ProjektId)
                                                .Select(z => z.NazivProjekta)
                                                .FirstOrDefaultAsync();
                }
                query = zf.Apply(query);

                if (zf.VrstaZahtjevaId.HasValue)
                {
                    zf.NazivVrsteZahtjeva = await ctx.vw_Zahtjevi
                                                     .Where(z => z.VrstaZahtjevaId == zf.VrstaZahtjevaId)
                                                     .Select(z => z.NazivVrsteZahtjeva)
                                                     .FirstOrDefaultAsync();
                }
                query = zf.Apply(query);
            }

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pageSize,
                TotalItems = count
            };

            if (count > 0 && (page < 1 || page > pagingInfo.TotalPages))
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending, filter });
            }

            query = query.ApplySort(sort, ascending);

            var zahtjevi = await query
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            for(int i = 0; i < zahtjevi.Count; i++)
            {
                zahtjevi[i].Position = (page-1) * pageSize + i;
            }

            var model = new ZahtjeviInfoViewModel
            {
                Zahtjevi = zahtjevi,
                PagingInfo = pagingInfo,
                Filter = zf
            };
			return View(model);
		}

        /// <summary>
        /// Filtrira Zahtjeve na temelju zadanih kriterija i preusmjerava na akciju Index s primijenjenim filtrima.
        /// </summary>
        /// <param name="filter">Objekt sa kriterijima za filtriranje.</param>
        /// <returns>Preusmjerava na akciju Index s primijenjenim filtrima.</returns>
        [HttpPost]
        public IActionResult Filter(ZahtjevFilter filter)
        {
            Console.WriteLine(filter);
            return RedirectToAction(nameof(Index), new { filter = filter.ToString() });
        }

        /// <summary>
        /// Prikazuje informacije o određenom Zahtjevu, uključujući detalje o povezanim zadacima.
        /// </summary>
        /// <param name="id">Identifikator Zahtjeva.</param>
        /// <param name="position">Pozicija na stranici.</param>
        /// <param name="filter">Filter za pretraživanje Zahtjeva.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Opcija sortiranja.</param>
        /// <param name="ascending">Redoslijed sortiranja.</param>
        /// <param name="viewName">Naziv pogleda.</param>
        /// <returns>View s informacijama o Zahtjevu i povezanim zadacima.</returns>
        public async Task<IActionResult> Show(int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            var zahtjev = await ctx.Zahtjevs
                                    .Where(z => z.ZahtjevId == id)
                                    .Select(z => new ZahtjevInfoViewModel
                                    {
                                        ZahtjevId = z.ZahtjevId,
                                        Oznaka = z.Oznaka,
                                        NazivZahtjeva = z.NazivZahtjeva,
                                        ProjektId = z.ProjektId,
                                        VrstaZahtjevaId = z.VrstaZahtjevaId,
                                        Prioritet = z.Prioritet
                                    })
                                    .FirstOrDefaultAsync();
            if (zahtjev == null)
            {
                return NotFound($"Dokument {id} ne postoji");
            }
            else
            {
                
                zahtjev.NazivProjekta = await ctx.vw_Zahtjevi
                                                .Where(p => p.ProjektId == zahtjev.ProjektId)
                                                .Select(p => p.NazivProjekta)
                                                .FirstOrDefaultAsync();

                zahtjev.NazivVrsteZahtjeva = await ctx.vw_Zahtjevi
                                                .Where(p => p.VrstaZahtjevaId == zahtjev.VrstaZahtjevaId)
                                                .Select(p =>p.NazivVrsteZahtjeva)
                                                .FirstOrDefaultAsync();

                //učitavanje zadataka
                var zadatci = await ctx.Zadataks
                                      .Where(z => z.ZahtjevId == zahtjev.ZahtjevId)
                                      .OrderBy(z => z.ZadatakId)
                                      .Select(z => new ZadatakViewModel
                                      {
                                          ZadatakId = z.ZadatakId,
                                          Opis = z.Opis,
                                          StatusZadatka = z.StatusZadatka.NazivStatusaZadatka
                                      })
                                      .ToListAsync();
                zahtjev.Zadatci = zadatci;

                Console.WriteLine("Učitavam editiranje...");
                foreach(var zad in zadatci)
                {
                    Console.WriteLine(zad.ZadatakId);
                }

                Console.WriteLine("Kraj učitavanja!");
                
                

                if (position.HasValue)
                {
                    page = 1 + position.Value / appData.PageSize;
                    await SetPreviousAndNext(position.Value, filter, sort, ascending);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;
                ViewBag.Position = position;

                return View(viewName, zahtjev);
            }
        }

        /// <summary>
        /// Postavlja informacije o prethodnom i sljedećem Zahtjevu na temelju trenutne pozicije.
        /// </summary>
        /// <param name="position">Trenutna pozicija.</param>
        /// <param name="filter">Filter za pretraživanje Zahtjeva.</param>
        /// <param name="sort">Opcija sortiranja.</param>
        /// <param name="ascending">Redoslijed sortiranja.</param>
        private async Task SetPreviousAndNext(int position, string filter, int sort, bool ascending)
        {
            var query = ctx.vw_Zahtjevi.AsQueryable();

            ZahtjevFilter df = new ZahtjevFilter();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                df = ZahtjevFilter.FromString(filter);
                if (!df.IsEmpty())
                {
                    query = df.Apply(query);
                }
            }

            query = query.ApplySort(sort, ascending);
            if (position > 0)
            {
                ViewBag.Previous = await query.Skip(position - 1).Select(z => z.ZahtjevId).FirstAsync();
            }
            if (position < await query.CountAsync() - 1)
            {
                ViewBag.Next = await query.Skip(position + 1).Select(z => z.ZahtjevId).FirstAsync();
            }
        }



        /// <summary>
        /// Prikazuje formu za kreiranje novog Zahtjeva.
        /// </summary>
        /// <returns>View za kreiranje Zahtjeva.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var zahtjev = new ZahtjevInfoViewModel();
            return View(zahtjev);
        }

        /// <summary>
        /// Pohranjuje novi Zahtjev u bazu podataka na temelju podataka iz modela.
        /// </summary>
        /// <param name="model">Podaci o novom Zahtjevu.</param>
        /// <returns>Preusmjerava na akciju Edit za uređivanje novog Zahtjeva ili ponovno prikazuje formu za unos ako neuspješno.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZahtjevInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                Zahtjev z = new Zahtjev();
                z.Oznaka = model.Oznaka;
                z.NazivZahtjeva = model.NazivZahtjeva;
                z.VrstaZahtjevaId = model.VrstaZahtjevaId;
                z.ProjektId = model.ProjektId;
                z.Prioritet = model.Prioritet;

                foreach (var zadatak in model.Zadatci)
                {
                    Zadatak noviZadatak = new Zadatak();
                    noviZadatak.Opis = zadatak.Opis;
                    noviZadatak.StatusZadatkaId = zadatak.StatusZadatkaId;
                    z.Zadataks.Add(noviZadatak);
                }
                try
                {
                    ctx.Add(z);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Zahtjev uspješno dodan. Id={z.ZahtjevId}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new { id = z.ZahtjevId });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);

            }
        }

        /// <summary>
        /// Prikazuje formu za uređivanje postojećeg Zahtjeva.
        /// </summary>
        /// <param name="id">ID Zahtjeva koji se uređuje.</param>
        /// <param name="position">Pozicija na stranici za paginaciju.</param>
        /// <param name="filter">Filter za pretraživanje.</param>
        /// <param name="page">Broj stranice za paginaciju.</param>
        /// <param name="sort">Sortiranje rezultata.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>View za uređivanje Zahtjeva.</returns>
        [HttpGet]
        public Task<IActionResult> Edit(int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            return Show(id, position, filter, page, sort, ascending, viewName: nameof(Edit));
        }

        /// <summary>
        /// Ažurira podatke o Zahtjevu u bazi podataka na temelju podataka iz modela.
        /// </summary>
        /// <param name="model">Podaci o Zahtjevu za ažuriranje.</param>
        /// <param name="position">Pozicija na stranici za paginaciju.</param>
        /// <param name="filter">Filter za pretraživanje.</param>
        /// <param name="page">Broj stranice za paginaciju.</param>
        /// <param name="sort">Sortiranje rezultata.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Preusmjerava na akciju Edit za uređivanje Zahtjeva ili ponovno prikazuje formu za uređivanje ako neuspješno.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ZahtjevInfoViewModel model, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Filter = filter;
            ViewBag.Position = position;
            if (ModelState.IsValid)
            {
                var zahtjev = await ctx.Zahtjevs
                                        .Include(d => d.Zadataks)
                                        .Where(d => d.ZahtjevId == model.ZahtjevId)
                                        .FirstOrDefaultAsync();
                if (zahtjev == null)
                {
                    return NotFound("Ne postoji zahtjev s id-om: " + model.ZahtjevId);
                }

                if (position.HasValue)
                {
                    page = 1 + position.Value / appData.PageSize;
                    await SetPreviousAndNext(position.Value, filter, sort, ascending);
                }

                zahtjev.Oznaka = model.Oznaka;
                zahtjev.NazivZahtjeva = model.NazivZahtjeva;
                zahtjev.VrstaZahtjevaId = model.VrstaZahtjevaId;
                zahtjev.ProjektId = model.ProjektId;
                zahtjev.Prioritet = model.Prioritet;


                List<int> idZadataka = model.Zadatci
                                            .Where(z => z.ZadatakId > 0)
                                            .Select(z => z.ZadatakId)
                                            .ToList();

                Console.WriteLine("Nakon submita");
                Console.WriteLine(model.Oznaka);
                foreach(var zad in model.Zadatci)
                {
                    Console.WriteLine(zad.ZadatakId);
                }

                Console.WriteLine(model.Zadatci.Count());

                // OVDJE JE NEKI PROBLEM!!!!!!
                // ctx.RemoveRange(zahtjev.Zadataks.Where(z => !idZadataka.Contains(z.ZadatakId)));

                foreach (var zadatak in model.Zadatci)
                {
                    Console.WriteLine(zadatak.ZadatakId);
                    //ažuriraj postojeće i dodaj nove
                    Zadatak noviZadatak; // potpuno nova ili dohvaćena ona koju treba izmijeniti
                    if (zadatak.ZadatakId > 0)
                    {
                        noviZadatak = zahtjev.Zadataks.First(z => z.ZadatakId == zadatak.ZadatakId);
                    }
                    else
                    {
                        Console.WriteLine("Ovdje");
                        noviZadatak = new Zadatak();
                        zahtjev.Zadataks.Add(noviZadatak);
                    }

                    noviZadatak.Opis = zadatak.Opis;
                    noviZadatak.StatusZadatkaId = zadatak.StatusZadatkaId;
                }

                try
                {

                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Zahtjev {zahtjev.ZahtjevId} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = zahtjev.ZahtjevId,
                        position,
                        filter,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }


        /// <summary>
        /// Briše Zahtjev iz baze podataka.
        /// </summary>
        /// <param name="ZahtjevId">ID Zahtjeva koji se briše.</param>
        /// <param name="filter">Filter za pretraživanje.</param>
        /// <param name="page">Broj stranice za paginaciju.</param>
        /// <param name="sort">Sortiranje rezultata.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Preusmjerava na akciju Index sa zadanim filterom, stranicom, sortiranjem i redoslijedom.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int ZahtjevId, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            var zahtjev = await ctx.Zahtjevs
                                    .Where(z => z.ZahtjevId == ZahtjevId)
                                    .SingleOrDefaultAsync();
            if (zahtjev != null)
            {
                try
                {
                    ctx.Remove(zahtjev);
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] = $"Zahtjev {zahtjev.ZahtjevId} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Ne postoji zahtjev s id-om: " + ZahtjevId;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { filter, page, sort, ascending });
        }

        /// <summary>
        /// Briše Zadatak iz baze podataka.
        /// </summary>
        /// <param name="id">ID Zadatka koji se briše.</param>
        /// <returns>Rezultat akcije koja šalje informacije o izvršenju brisanja klijentskoj strani.</returns>
        public async Task<IActionResult> DeleteZadatak(int id)
        {
            ActionResponseMessage responseMessage;
            var zadatak = await ctx.Zadataks.FindAsync(id);
            Console.WriteLine("");
            if (zadatak != null)
            {
                try
                {
                    ctx.Remove(zadatak);
                    await ctx.SaveChangesAsync();
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Zadatak sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja zadatka: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Zadatak sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
              new EmptyResult() : await Get(id);
        }

        /// <summary>
        /// Dohvaća Zadatak iz baze podataka prema ID-u.
        /// </summary>
        /// <param name="id">ID Zadatka koji se dohvaća.</param>
        /// <returns>Parcijalni pogled s podacima o Zadatku ili NotFound ako Zadatak s navedenim ID-om ne postoji.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var zadatak = await ctx.Zadataks
                                  .Where(m => m.ZadatakId == id)
                                  .Select(m => new ZadatakViewModel
                                  {
                                      ZadatakId = m.ZadatakId,
                                      StatusZadatkaId = m.StatusZadatkaId,
                                      Opis = m.Opis,
                                      StatusZadatka = m.StatusZadatka.NazivStatusaZadatka
                                  })
                                  .SingleOrDefaultAsync();
            if (zadatak != null)
            {
                return PartialView(zadatak);
            }
            else
            {
                return NotFound($"Neispravan id mjesta: {id}");
            }
        }
    }
}

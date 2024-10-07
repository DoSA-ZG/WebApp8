using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure.Identity;
using NLog.Filters;

namespace RPPP_WebApp.Controllers
{
    public class ZzadatakController : Controller
    {
        private readonly Rppp08Context ctx;
        private readonly ILogger<ZzadatakController> logger;
        private readonly AppSettings appSettings;

        public ZzadatakController(Rppp08Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZzadatakController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                int pagesize = appSettings.PageSize;

                var query = ctx.Zadataks
                            .Include(z => z.Zahtjev)
                            .Include(z => z.StatusZadatka)
                            .Include(z => z.ZadatakSuradniks)
                                .ThenInclude(zs => zs.Suradnik)
                            .AsNoTracking();

                int count = query.Count();
                if (count == 0)
                {
                    logger.LogInformation("Ne postoji niti jedan zadatak");
                    return RedirectToAction(nameof(Create));
                }

                var pagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    Sort = sort,
                    Ascending = ascending,
                    ItemsPerPage = pagesize,
                    TotalItems = count
                };
                if (page < 1 || page > pagingInfo.TotalPages)
                {
                    return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
                }
                query = query.ApplySort(sort, ascending);


                var zadaci = await query
                              .Skip((page - 1) * pagesize)
                              .Take(pagesize)
                              .ToListAsync();

                var model = new ZzadaciViewModel
                {
                    Zadaci = zadaci,
                    PagingInfo = pagingInfo
                };

                return View(model);
            }
            catch (Exception exc)
            {
                logger.LogError($"Dogodila se greška prilikom učitavanja Index: {exc}");
                TempData["StatusMessage"] = "Pogreška prilikom dohvata podataka";
                return RedirectToAction(nameof(Index));
            }
        }


        private async Task PrepareDropDownLists()
        {
            var zahtjevi = await ctx.Zahtjevs
                                    .Select(z => new { z.ZahtjevId, z.NazivZahtjeva })
                                    .ToListAsync();
            ViewBag.Zahtjevi = new SelectList(zahtjevi, nameof(Zahtjev.ZahtjevId), nameof(Zahtjev.NazivZahtjeva));

            var statusiZadataka = await ctx.StatusZadatkas
                                  .Select(s => new { s.StatusZadatkaId, s.NazivStatusaZadatka })
                                  .ToListAsync();
            ViewBag.Statusi = new SelectList(statusiZadataka, nameof(StatusZadatka.StatusZadatkaId), nameof(StatusZadatka.NazivStatusaZadatka));
        }

        public async Task<IActionResult> Details(int id)
        {
            var zadatak = await ctx.Zadataks
                .Include(z => z.Zahtjev)
                .Include(z => z.StatusZadatka)
                .Include(z => z.ZadatakSuradniks)
                    .ThenInclude(zs => zs.Suradnik)
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.ZadatakId == id);

            if (zadatak == null)
            {
                return NotFound();
            }

            return View(zadatak);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateP(Zadatak zadatak)
        {
            logger.LogTrace(JsonSerializer.Serialize(zadatak));
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        ctx.Add(zadatak);
                        ctx.SaveChanges();
                        logger.LogInformation($"Zadatak \"{zadatak.Opis}\" uspješno dodan.");
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom dodavanje zadatka");
                        return View(zadatak);
                    }
                }
                await PrepareDropDownLists();
                return View();
            }
            catch (Exception exc)
            {
                logger.LogError($"Dogodila se greška u CreateP: {exc}");
                TempData["StatusMessage"] = "Pogreška prilikom izrade zadatka";
                return View(zadatak);
            }
        }

        public async Task<IActionResult> Show(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            Zadatak zad = await ctx.Zadataks
                            .Include(z => z.Zahtjev)
                            .Include(z => z.StatusZadatka)
                            .Include(z => z.ZadatakSuradniks)
                            .Where(z => z.ZadatakId == id)
                            .FirstOrDefaultAsync();
            if (zad == null)
            {
                return NotFound("Zadatak " + id + " ne postoji.");
            }

            ZzadatakViewModel zadatak = new ZzadatakViewModel();

            zadatak.zadatak = zad;

            var suradnici = await ctx.ZadatakSuradniks
                                        .Where(zs => zs.ZadatakId == id)
                                        .Select(s => new ZsuradnikViewModel
                                        {
                                            Suradnik = s.Suradnik,
                                            AvailableZadaci = ctx.Zadataks.ToList(),
                                            SelectedZadaci = ctx.ZadatakSuradniks
                                                                .Where(relatedZs => relatedZs.SuradnikId == s.SuradnikId)
                                                            .Select(relatedZs => relatedZs.ZadatakId)
                                                            .ToArray()
                                        })
                                        .ToListAsync();
            zadatak.zsuradnikViewModels = suradnici;

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            await PrepareDropDownLists();
            return View(viewName, zadatak);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            return await Show(id, page, sort, ascending, viewName: nameof(Edit));
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ZzadatakViewModel zz, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;

            if (ModelState.IsValid)
            {
                Zadatak zadatak = await ctx.Zadataks
                                            .Where(z => z.ZadatakId == zz.zadatak.ZadatakId)
                                            .FirstOrDefaultAsync();

                if (zadatak == null)
                {
                    return NotFound("Ne postoji zadatak sa id-om: " + zz.zadatak.ZadatakId);
                }
                
                zadatak.StatusZadatkaId = zz.zadatak.StatusZadatkaId;
                zadatak.StatusZadatka = ctx.StatusZadatkas.Find(zz.zadatak.StatusZadatkaId);
                
                zadatak.Opis = zz.zadatak.Opis;
                zadatak.Zahtjev = ctx.Zahtjevs.Find(zz.zadatak.ZahtjevId);

                List<int> list = zz.zsuradnikViewModels.Select(zsvm => zsvm.Suradnik.SuradnikId).ToList();
                ctx.RemoveRange(ctx.ZadatakSuradniks.Where(zs => zs.ZadatakId == zz.zadatak.ZadatakId && !list.Contains(zs.SuradnikId)));

                foreach (var zsv in zz.zsuradnikViewModels)
                {
                    Suradnik suradnik;
                    ZadatakSuradnik zasu;

                    try
                    {
                        zasu = ctx.ZadatakSuradniks.Where(z => z.ZadatakId == zz.zadatak.ZadatakId && z.SuradnikId == zsv.Suradnik.SuradnikId).First();
                        suradnik = ctx.Suradniks.Where(s => s.SuradnikId == zsv.Suradnik.SuradnikId).First();
                    }
                    catch (InvalidOperationException exc)
                    {

                        suradnik = ctx.Suradniks.Find(zsv.Suradnik.SuradnikId);
                        if (suradnik == null)
                        {
                            suradnik = new Suradnik();
                        }
                        zasu = new ZadatakSuradnik();
                    }
                    zasu.ZadatakId = zadatak.ZadatakId;
                    zasu.SuradnikId = suradnik.SuradnikId;
                    zasu.Suradnik = suradnik;
                    zasu.Zadatak = zadatak;
                    suradnik.ZadatakSuradniks.Add(zasu);
                    zadatak.ZadatakSuradniks.Add(zasu);

                    suradnik.BrojMobitela = zsv.Suradnik.BrojMobitela;
                    suradnik.Email = zsv.Suradnik.Email;
                    suradnik.Ime = zsv.Suradnik.Ime;
                    suradnik.Prezime = zsv.Suradnik.Prezime;
                }

                try
                {

                    await ctx.SaveChangesAsync();
                    logger.LogInformation("Zadatak " + zadatak.Opis + " uspješno ažuriran.");
                    TempData[Constants.Message] = $"Zadatak {zz.zadatak.ZadatakId} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = zz.zadatak.ZadatakId,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                    return View(zz);
                }
            }
            else
            {
                return View(zz);
            }
        }




        [HttpPost]
        public IActionResult Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var zadatak = ctx.Zadataks
                .Include(z => z.ZadatakSuradniks)
                .Where(z => z.ZadatakId == id)
                .SingleOrDefault();

            if (zadatak != null)
            {
                try
                {
                    ctx.RemoveRange(zadatak.ZadatakSuradniks);
                    ctx.Remove(zadatak);
                    ctx.SaveChanges();
                    logger.LogInformation($"Zadatak \"{zadatak.Opis}\" uspješno obrisan");

                    TempData["StatusMessage"] = $"Zadatak \"{zadatak.Opis}\" uspješno obrisan";
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja zadatka.");
                    TempData["StatusMessage"] = "Pogreška prilikom brisanja zadatka";
                }
            }
            else
            {
                logger.LogWarning("Ne postoji zadatak koji ima id " + id);
                TempData["StatusMessage"] = "Zadatak ne postoji.";
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        [HttpPost]
        public IActionResult DeleteSuradnik(int suradnikId, int zadatakId)
        {
            var suradnik = ctx.Suradniks.Include(s => s.ZadatakSuradniks).FirstOrDefault(s => s.SuradnikId == suradnikId);

            if (suradnik != null)
            {
                try
                {
                    string sur = suradnik.Ime + " " + suradnik.Prezime;

                    ctx.ZadatakSuradniks.RemoveRange(suradnik.ZadatakSuradniks);

                    ctx.Suradniks.Remove(suradnik);

                    ctx.SaveChanges();

                    logger.LogInformation($"Suradnik {sur} uspješno obrisan");
                    TempData["StatusMessage"] = $"Suradnik {sur} uspješno obrisan";
                }
                catch (Exception exc)
                {
                    logger.LogError($"Pogreška prilikom brisanja suradnika: {exc.Message}");
                    TempData["StatusMessage"] = "Pogreška prilikom brisanja suradnika";
                }
            }
            else
            {
                logger.LogWarning("Ne postoji suradnik koji ima id " + suradnikId);
            }

            return RedirectToAction(nameof(Edit));
        }

    }
}

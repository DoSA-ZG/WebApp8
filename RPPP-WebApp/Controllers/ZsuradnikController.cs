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

namespace RPPP_WebApp.Controllers
{
    public class ZsuradnikController : Controller
    {

        private readonly Rppp08Context ctx;
        private readonly ILogger<ZsuradnikController> logger;
        private readonly AppSettings appSettings;

        public ZsuradnikController(Rppp08Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZsuradnikController> logger)
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

                var query = ctx.Suradniks.AsNoTracking();

                int count = query.Count();
                if (count == 0)
                {
                    logger.LogInformation("Ne postoji niti jedan suradnik");
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


                var suradnici = await query
                                    .Skip((page - 1) * pagesize)
                                    .Take(pagesize)
                                    .ToListAsync();

                var model = new ZsuradniciViewModel
                {
                    Suradnici = suradnici,
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

        [HttpGet]
        public async Task<IActionResult> Create(int zadatakId = -1)
        {
            var availableZadaci = ctx.Zadataks.ToList();

            if (zadatakId < 0)
            {
                var viewModel = new ZsuradnikViewModel
                {
                    AvailableZadaci = availableZadaci,
                    SelectedZadaci = new int[] { }
                };
                return View(viewModel);
            }
            else
            {
                var viewModel = new ZsuradnikViewModel
                {
                    AvailableZadaci = availableZadaci,
                    SelectedZadaci = new int[] { zadatakId }
                };
                return View(viewModel);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZsuradnikViewModel zsuradnik)
        {
            if (ModelState.IsValid)
            {
                Suradnik suradnik = zsuradnik.Suradnik;

                if (zsuradnik.SelectedZadaci != null)
                {
                    foreach (var zadakId in zsuradnik.SelectedZadaci)
                    {
                        var zadatak = ctx.Zadataks.Find(zadakId);
                        if (zadatak != null)
                        {
                            var zadatakSuradnik = new ZadatakSuradnik
                            {
                                Zadatak = zadatak,
                                Suradnik = suradnik
                            };

                            suradnik.ZadatakSuradniks.Add(zadatakSuradnik);
                        }
                    }
                }
                ctx.Suradniks.Add(suradnik);
                ctx.SaveChanges();
                logger.LogInformation("Suradnik " + suradnik.Ime + " " + suradnik.Prezime + " uspješno dodan");
                return RedirectToAction("Index");
            }
            zsuradnik.AvailableZadaci = ctx.Zadataks.ToList();
            return View(zsuradnik);

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suradnik = await ctx.Suradniks
                .Include(s => s.ZadatakSuradniks)
                .ThenInclude(zs => zs.Zadatak)
                .FirstOrDefaultAsync(s => s.SuradnikId == id);

            if (suradnik == null)
            {
                return NotFound();
            }
            List<int> list = new List<int>();
            foreach (var z in suradnik.ZadatakSuradniks)
            {
                list.Add(z.ZadatakId);
            }
            var viewModel = new ZsuradnikViewModel
            {
                Suradnik = suradnik,
                AvailableZadaci = ctx.Zadataks.ToList(),
                SelectedZadaci = list.ToArray()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id, ZsuradnikViewModel viewModel)
        {
            if (id == null)
            {
                return NotFound("Ne postoji suradnik s id-om: " + id);
            }

            var suradnikToUpdate = await ctx.Suradniks
                .Include(s => s.ZadatakSuradniks)
                .Where(s => s.SuradnikId == id)
                .FirstOrDefaultAsync();
            if (suradnikToUpdate == null)
            {
                return NotFound("Ne postoji suradnik s id-om: " + id);
            }
            suradnikToUpdate.BrojMobitela = viewModel.Suradnik.BrojMobitela;
            suradnikToUpdate.Email = viewModel.Suradnik.Email;
            suradnikToUpdate.Ime = viewModel.Suradnik.Ime;
            suradnikToUpdate.Prezime = viewModel.Suradnik.Prezime;
            ctx.SaveChanges();

            if (await TryUpdateModelAsync<Suradnik>(suradnikToUpdate, "",
                s => s.Ime, s => s.Prezime, s => s.Email, s => s.BrojMobitela))
            {
                UpdateZadaci(suradnikToUpdate, viewModel.SelectedZadaci);

                try
                {
                    await ctx.SaveChangesAsync();
                    logger.LogInformation("Suradnik " + suradnikToUpdate.Ime + " " + suradnikToUpdate.Prezime + " uspješno ažuriran.");
                    TempData["StatusMessage"] = $"Suradnik \"{suradnikToUpdate.Ime} {suradnikToUpdate.Prezime}\" uspješno ažuriran";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                }
            }

            viewModel.AvailableZadaci = ctx.Zadataks.ToList();
            return View(viewModel);
        }

        private void UpdateZadaci(Suradnik suradnik, int[] selectedZadaci)
        {
            if (selectedZadaci == null)
            {
                suradnik.ZadatakSuradniks = new List<ZadatakSuradnik>();
                return;
            }

            var selectedZadaciHS = new HashSet<int>(selectedZadaci);
            var existingZadaciHS = new HashSet<int>(suradnik.ZadatakSuradniks.Select(zs => zs.ZadatakId));

            foreach (var zadaci in ctx.Zadataks)
            {
                if (selectedZadaciHS.Contains(zadaci.ZadatakId))
                {
                    if (!existingZadaciHS.Contains(zadaci.ZadatakId))
                    {
                        suradnik.ZadatakSuradniks.Add(new ZadatakSuradnik { ZadatakId = zadaci.ZadatakId, SuradnikId = suradnik.SuradnikId });
                    }
                }
                else
                {
                    if (existingZadaciHS.Contains(zadaci.ZadatakId))
                    {
                        var zadaciToRemove = suradnik.ZadatakSuradniks.FirstOrDefault(zs => zs.ZadatakId == zadaci.ZadatakId);
                        if (zadaciToRemove != null)
                        {
                            ctx.Remove(zadaciToRemove);
                        }
                    }
                }
            }
        }


        [HttpPost]
        public IActionResult Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var suradnik = ctx.Suradniks.Include(s => s.ZadatakSuradniks).FirstOrDefault(s => s.SuradnikId == id);

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
                logger.LogWarning("Ne postoji suradnik koji ima id " + id);
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }
    }
}

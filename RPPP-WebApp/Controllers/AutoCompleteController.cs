using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class AutoCompleteController : Controller
    {

        private readonly Rppp08Context ctx;
        private readonly AppSettings appData;

        public AutoCompleteController(Rppp08Context ctx, IOptionsSnapshot<AppSettings> options)
        {
            this.ctx = ctx;
            appData = options.Value;
        }
        public async Task<IEnumerable<IdLabel>> Projekt(string term)
        {
            var query = ctx.Projekts
                               .Select(p => new IdLabel
                               {
                                   Id = p.ProjektId,
                                   Label = p.NazivProjekta
                               })
                               .Where(p => p.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                    .ThenBy(l => l.Id)
                                    .Take(appData.AutoCompleteCount)
                                    .ToListAsync();

            return list;
        }

        public async Task<IEnumerable<IdLabel>> StatusZadatka(string term)
        {
            var query = ctx.StatusZadatkas
                            .Select(p => new IdLabel
                            {
                                Id = p.StatusZadatkaId,
                                Label = p.NazivStatusaZadatka
                            })
                            .Where(p => p.Label.Contains (term));

            var list = await query.OrderBy(l => l.Label)
                                       .ThenBy(l => l.Id) 
                                       .Take(appData.AutoCompleteCount)
                                       .ToListAsync();

            return list;
        }

        public async Task<IEnumerable<IdLabel>> VrstaZahtjeva(string term)
        {
            var query = ctx.VrstaZahtjevas
                            .Select(p => new IdLabel
                            {
                                Id = p.VrstaZahtjevaId,
                                Label = p.NazivVrsteZahtjeva
                            })
                            .Where(p => p.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                                       .ThenBy(l => l.Id)
                                       .Take(appData.AutoCompleteCount)
                                       .ToListAsync();

            return list;
        }

        public async Task<IEnumerable<AutoCompleteZsuradnik>> Zsuradnik(string term)
        {
            var query = ctx.Suradniks
                            .Where(a => a.Ime.Contains(term))
                            .OrderBy(a => a.Ime)
                            .Select(a => new AutoCompleteZsuradnik
                            {
                                Id = a.SuradnikId,
                                Label = a.Ime + " " + a.Prezime + " (" + a.Email + ")",
                                Ime = a.Ime,
                                Prezime = a.Prezime,
                                Email = a.Email,
                                BrojMobitela = a.BrojMobitela
                            });
            var list = await query.OrderBy(l => l.Label)
                                    .ThenBy(l => l.Prezime)
                                    .ThenBy(l => l.Email)
                                    .ThenBy(l => l.BrojMobitela)
                                    .Take(appData.AutoCompleteCount)
                                    .ToListAsync();
            return list;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;


namespace RPPP_WebApp.Controllers
{
	public class ProjektController : Controller
	{
		private readonly Rppp08Context _db;

		public ProjektController(Rppp08Context db)
		{
			_db = db;
		}

		public IActionResult Index()
		{
			var projekt = _db.Projekts.ToList();
			var dokumentacijaList = _db.Dokumentacijas.ToList();
            var vp = _db.VrstaProjekta.ToList();

            var viewModel = new PDViewModel
            {
                ProjektData = projekt,
                DokumentacijaData = dokumentacijaList,
                VrstaProjektaData = vp
            };
            return View(viewModel);
		}

        public IActionResult Create()
        {
            ViewBag.VrsteProjekta = _db.VrstaProjekta.ToList();
            return View();
        }

        //POST
        [HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Projekt obj)
		{
			_db.Projekts.Add(obj);
			_db.SaveChanges();
			TempData["success"] = "Projekt uspješno stvoren";
			return RedirectToAction("Index");
		}

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var projekt = _db.Projekts.Find(id);

            if (projekt == null)
            {
                return NotFound("Ne postoji projekt s id " + id);
            }
            ViewBag.VrsteProjekta = _db.VrstaProjekta.ToList();

            return View(projekt);
        }

        public IActionResult Edit(Projekt model)
        {
            try
            {
                var existingProjekt = _db.Projekts.Find(model.ProjektId);

                if (existingProjekt != null)
                {
                    existingProjekt.OpisProjekta = model.OpisProjekta;
                    existingProjekt.NazivProjekta = model.NazivProjekta;
                    existingProjekt.PlaniraniPocetak = model.PlaniraniPocetak;
                    existingProjekt.StvarniPocetak = model.StvarniPocetak;
                    existingProjekt.PlaniraniZavrsetak = model.PlaniraniZavrsetak;
                    existingProjekt.StvarniZavrsetak = model.StvarniZavrsetak;
                    existingProjekt.VrstaProjektaId = model.VrstaProjektaId;

                    _db.Projekts.Update(existingProjekt);
                    _db.SaveChanges();

                    TempData["success"] = "Projekt uspješno ažuriran";
                    return RedirectToAction("Details", new { id = existingProjekt.ProjektId });
                }
                else
                {
                    return NotFound("Ne postoji projekt s ID " + model.ProjektId);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Došlo je do greške prilikom ažuriranja projekta.";
                return NotFound("error" + model.ProjektId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEdit(ProjektDokumentacijaViewModel model)
        {
                try
                {
                    var existingProjekt = _db.Projekts.Find(model.ProjektData.ProjektId);

                    if (existingProjekt != null)
                    {
                        existingProjekt.OpisProjekta = model.ProjektData.OpisProjekta;
                        existingProjekt.NazivProjekta = model.ProjektData.NazivProjekta;
                        existingProjekt.PlaniraniPocetak = model.ProjektData.PlaniraniPocetak;
                        existingProjekt.StvarniPocetak = model.ProjektData.StvarniPocetak;
                        existingProjekt.PlaniraniZavrsetak = model.ProjektData.PlaniraniZavrsetak;
                        existingProjekt.StvarniZavrsetak = model.ProjektData.StvarniZavrsetak;
                        existingProjekt.VrstaProjektaId = model.ProjektData.VrstaProjektaId;

                        _db.Projekts.Update(existingProjekt);
                        _db.SaveChanges();

                        TempData["success"] = "Projekt uspješno ažuriran";
                        return RedirectToAction("Details", new { id = existingProjekt.ProjektId });
                    }
                    else
                    {
                        return NotFound("Ne postoji projekt s ID " + model.ProjektData.ProjektId);
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Došlo je do greške prilikom ažuriranja projekta.";
                    return NotFound("error" + model.ProjektData.ProjektId);
                }
        }

        [HttpPost]
		public IActionResult Delete(int id)
		{
		
			var projekt = _db.Projekts.Find(id);

			try
			{
				_db.Remove(projekt);
				_db.SaveChanges();
				TempData["Message"] = $"Obrisan je {projekt.NazivProjekta}.";
			}
			catch (Exception exc)
			{
				TempData["Message"] = "Neuspješno izbrisan projekt";
			}
			return RedirectToAction("Index");


		}

		public IActionResult Details(int id) {

			var projekt = _db.Projekts.Find(id);
            var dokumentacijaList = _db.Dokumentacijas.Where(d => d.ProjektId == id).ToList();
            var vp = _db.VrstaDokumentacijes.ToList();
        


            var viewModel = new ProjektDokumentacijaViewModel
			{
				ProjektData = projekt,
                DokumentacijaData = dokumentacijaList,
                VrstaDokumentacije = vp
            };
            ViewBag.VrsteProjekta = _db.VrstaProjekta.ToList();
            ViewBag.VrsteDokumentacije = _db.VrstaDokumentacijes.ToList();
            return View(viewModel);

		}


        [HttpPost]
        public IActionResult DeleteDoc(int id)
        {

            var dokumentacija = _db.Dokumentacijas.Find(id);

            try
            {
                _db.Remove(dokumentacija);
                _db.SaveChanges();
                TempData["Message"] = $"Obrisana je {dokumentacija.NazivDokumentacije}.";
                return RedirectToAction("Details", new { id = dokumentacija.ProjektId });
            }
            catch (Exception exc)
            {
                TempData["Message"] = "Neuspješno izbrisana dokumentacija";
            }
            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDoc(ProjektDokumentacijaViewModel model)
        {
            try
            {
                return NotFound(model);
                
            }
            catch (Exception ex)
            {
                TempData["error"] = "Došlo je do greške prilikom ažuriranja dokumentacije.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDokumentacija(ProjektDokumentacijaViewModel obj)
        {
            Dokumentacija m = new Dokumentacija();
            m.ProjektId = obj.ProjektData.ProjektId;
            m.NazivDokumentacije = obj.NewDokumentacija.NazivDokumentacije;
            m.VrstaDokumentacijeId = obj.NewDokumentacija.VrstaDokumentacijeId;

            _db.Dokumentacijas.Add(m);
            _db.SaveChanges();
            TempData["success"] = "Projekt uspješno stvoren";

            // Redirect to a different action or page
            return RedirectToAction("Details", new { id = obj.ProjektData.ProjektId });
        }




        public async Task<IEnumerable<IdLabel>> Projekt(string term) {

            var query = _db.Projekts
            .Select(p => new IdLabel {
                Id = p.ProjektId,
                Label = p.NazivProjekta
            })
            .Where(p => p.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                .Take(5)
                .ToListAsync();
            return list;
    }


	}

}
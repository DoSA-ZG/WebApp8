using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers

{
    public class DokumentacijaController : Controller
    {
        private readonly Rppp08Context _db;

        public DokumentacijaController(Rppp08Context db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var dokumentacijaList = _db.Dokumentacijas.ToList();
            var vrstaList = _db.VrstaDokumentacijes.ToList();

            var viewModel = new DokumentacijaViewModel
            {
                DokumentacijaData = dokumentacijaList,
                VrstaData = vrstaList,
            };
            return View(viewModel);
        }

        public IActionResult Create()
        {
            var vrsteDokumentacije = _db.VrstaDokumentacijes.ToList();
            ViewBag.VrsteDokumentacije = vrsteDokumentacije;
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Dokumentacija obj)
        {
            _db.Dokumentacijas.Add(obj);
            _db.SaveChanges();
            TempData["success"] = "Projekt uspješno stvoren";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {

            var dokumentacija = _db.Dokumentacijas.Find(id);

            try
            {
                _db.Remove(dokumentacija);
                _db.SaveChanges();
                TempData["Message"] = $"Obrisana je {dokumentacija.NazivDokumentacije}.";
            }
            catch (Exception exc)
            {
                TempData["Message"] = "Neuspješno izbrisana dokumentacija";
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var dokumentacija = _db.Dokumentacijas.Find(id);

            if (dokumentacija == null)
            {
                return NotFound("Ne postoji dokumentacija s ID " + id);
            }

            var vrsteDokumentacije = _db.VrstaDokumentacijes.ToList();
            ViewBag.VrsteDokumentacije = vrsteDokumentacije;

            return View(dokumentacija);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Dokumentacija model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingDokumentacija = _db.Dokumentacijas.Find(model.DokumentacijaId);

                    if (existingDokumentacija != null)
                    {
                        existingDokumentacija.NazivDokumentacije = model.NazivDokumentacije;
                        existingDokumentacija.ProjektId = model.ProjektId;
                        existingDokumentacija.VrstaDokumentacijeId = model.VrstaDokumentacijeId;

                        _db.Dokumentacijas.Update(existingDokumentacija);
                        _db.SaveChanges();

                        TempData["success"] = "Dokumentacija uspješno ažurirana";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return NotFound("Ne postoji dokumentacija s ID " + model.DokumentacijaId);
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Došlo je do greške prilikom ažuriranja dokumentacije.";
                    return View(model);
                }
            }

            return View(model);
        }



    }
}

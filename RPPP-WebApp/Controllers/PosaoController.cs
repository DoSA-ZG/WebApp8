using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions;


namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje operacijama vezanim uz poslove.
    /// </summary>
    public class PosaoController : Controller
    {
        private readonly Rppp08Context _context;

        /// <summary>
        /// Konstruktor kontrolera s primarnim kontekstom baze podataka.
        /// </summary>
        public PosaoController(Rppp08Context context)
        {
            _context = context;
        }


        // GET: Posao
        /// <summary>
        /// Prikazuje popis poslova.
        /// </summary>
        /// <returns>View s popisom poslova.</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Posaos.Include(s => s.Suradniks).Include(p => p.VrstaPosla).Include(p => p.Projekt).ToListAsync());
        }

        /// <summary>
        /// Priprema padajuće popise za dodavanje novog posla.
        /// </summary>
        /// <returns>Task rezultat.</returns>
        private async Task PrepareDropDownLists()
        {
            var projekti = await _context.Projekts
            .Select(p => new { p.ProjektId, p.NazivProjekta }).ToListAsync();
            ViewBag.Projekti = new SelectList(projekti,
            "ProjektId", "NazivProjekta");

            var vrstePosla = await _context.VrstaPoslas
            .Select(p => new { p.VrstaPoslaId, p.NazivVrste }).ToListAsync();
            ViewBag.VrstePosla = new SelectList(vrstePosla,
            "VrstaPoslaId", "NazivVrste");
        }

        /// <summary>
        /// Prikazuje formu za dodavanje novog posla.
        /// </summary>
        /// <returns>View s formom za dodavanje novog posla.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Post metoda za stvaranje novog posla.
        /// </summary>
        /// <param name="posao">Posao koji se dodaje.</param>
        /// <returns>Redirekcija na Index ili View s porukom o grešci.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Posao posao)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(posao);
                    _context.SaveChanges();

                    TempData[Constants.Message] =
                    $"Posao s opisom '{posao.Opis}' dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction("Index");

                }
                catch (Exception exc)
                {

                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());

                    await PrepareDropDownLists();
                    return View("Create", posao);
                }
            }

            await PrepareDropDownLists();
            return View("Create", posao);


        }

        /// <summary>
        /// Dodaje redak posla u tablicu s dodatnim informacijama o suradnicima.
        /// </summary>
        /// <param name="posao">Posao koji se dodaje.</param>
        /// <param name="SuradnikId">ID suradnika koji je povezan s poslom.</param>
        /// <returns>Akcija rezultata koja predstavlja odgovor na zahtjev.</returns>
        [HttpPost]
        public async Task<IActionResult> AddRow(Posao posao, int SuradnikId)
        {

            ViewBag.SuradnikId = SuradnikId;
            if (!ModelState.IsValid)
            {

                if (posao.VrstaPoslaId == null)
                {

                    ModelState.AddModelError("VrstaPoslaId", "Vrsta posla mora biti odabrana.");

                }
                else
                {
                    posao.VrstaPosla = _context.VrstaPoslas.Find(posao.VrstaPoslaId);
                    ViewBag.NazivPosla = _context.VrstaPoslas.Find(posao.VrstaPoslaId).NazivVrste;

                }


                await PrepareDropDownLists();
                return PartialView("GetAddRow", posao);
            }

            if (ModelState.IsValid)
            {

                if (posao.VrstaPoslaId == null)
                {

                    ModelState.AddModelError("VrstaPoslaId", "Vrsta posla mora biti odabrana.");
                    return PartialView("GetAddRow", posao);
                }

                try
                {
                    _context.Add(posao);
                    _context.SaveChanges();

                    var posao2 = await _context.Posaos
                .Where(p => p.PosaoId == posao.PosaoId).Include(p => p.VrstaPosla).FirstOrDefaultAsync();
                    return PartialView("Get", posao);

                }
                catch (Exception exc)
                {


                    await PrepareDropDownLists();
                    return PartialView("GetAddRow", posao);

                }
            }


            await PrepareDropDownLists();
            return BadRequest("ERORR OCCURED WITH VALIDATION");


        }

        /// <summary>
        /// Briše posao.
        /// </summary>
        /// <param name="PosaoId">ID posla koji se briše.</param>
        /// <returns>Redirekcija na akciju Index.</returns>
        [HttpPost]
        public IActionResult Delete(int PosaoId)
        {

            var posao = _context.Posaos.Find(PosaoId);

            try
            {
                _context.Remove(posao);
                _context.SaveChanges();
                TempData[Constants.Message] =
                       $"Posao s opisom '{posao.Opis}' obrisan.";
                TempData[Constants.ErrorOccurred] = false;
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = $"Neuspjeh pri brisanju posla.";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction("Index");

        }

        /// <summary>
        /// Izlazi iz dodavanja reda posla.
        /// </summary>
        /// <param name="TransakcijaId">ID transakcije.</param>
        /// <returns>Prazan rezultat.</returns>
        [HttpDelete]
        public async Task<IActionResult> QuitAddRow(int TransakcijaId)
        {

            return new EmptyResult();

        }

        /// <summary>
        /// Prikazuje formu za uređivanje posla.
        /// </summary>
        /// <param name="id">ID posla koji se uređuje.</param>
        /// <returns>View s formom za uređivanje posla.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var posao = _context.Posaos.Find(id);

            if (posao == null)
            {
                return NotFound("Ne postoji posao s id " + id);
            }

            await PrepareDropDownLists();
            return View(posao);
        }

        /// <summary>
        /// Post metoda za ažuriranje posla.
        /// </summary>
        /// <param name="id">ID posla koji se ažurira.</param>
        /// <returns>View s formom za uređivanje posla ili redirekcija na Index.</returns>
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Update(int id)
        {
            var posao = await _context.Posaos.Where(p => p.PosaoId == id).FirstOrDefaultAsync();

            if (posao == null)
            {
                return NotFound("Ne postoji posao s id " + id);
            }

            if (await TryUpdateModelAsync<Posao>(posao, "", p => p.VrstaPosla))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData[Constants.Message] = "Posao uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Neuspjeh pri spremanju podataka.. ");
                }
            }

            await PrepareDropDownLists();
            return View(posao);
        }

        /// <summary>
        /// Prikazuje formu za uređivanje reda posla.
        /// </summary>
        /// <param name="id">ID posla čiji se red uređuje.</param>
        /// <returns>PartialView s formom za uređivanje reda posla.</returns>
        [HttpGet]
        public async Task<IActionResult> EditRow(int id)
        {   
            var posao = await _context.Posaos.Include(s => s.Suradniks).Include(p => p.VrstaPosla).Include(p => p.Projekt).Where(p => p.PosaoId == id).FirstOrDefaultAsync();


            if (posao == null) {
                return NotFound("Ne postoji posao s id " + id);
            }

            await PrepareDropDownLists();
            return PartialView(posao);
        }

        /// <summary>
        /// Prikazuje formu za dodavanje reda posla.
        /// </summary>
        /// <param name="SuradnikId">ID suradnika koji je povezan s poslom.</param>
        /// <returns>PartialView s formom za dodavanje reda posla.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAddRow(int SuradnikId)
        {

            ViewBag.SuradnikId = SuradnikId;
            await PrepareDropDownLists();
            return PartialView();
        }

        /// <summary>
        /// Prikazuje informacije o određenom poslu.
        /// </summary>
        /// <param name="id">ID posla čije se informacije prikazuju.</param>
        /// <returns>PartialView s informacijama o poslu.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id) {
        var posao = await _context.Posaos
            .Where(p => p.PosaoId == id).Include(s => s.Suradniks).Include(p => p.VrstaPosla).Include(p => p.Projekt).FirstOrDefaultAsync();

        if (posao != null)
        return PartialView(posao);
        else
        return NotFound($"Neispravan id posla: {id}");
        }

        /// <summary>
        /// Post metoda za ažuriranje reda posla.
        /// </summary>
        /// <param name="posao">Posao koji se ažurira.</param>
        /// <returns>PartialView s informacijama o ažuriranom poslu.</returns>
        [HttpPost]
        public async Task<IActionResult> EditRow(Posao posao) {
           
            if (!ModelState.IsValid) {


                await PrepareDropDownLists();
               
                return PartialView(posao);
            }
        


            try {
                _context.Update(posao);
                await _context.SaveChangesAsync();
                var posao2 = await _context.Posaos
            .Where(p => p.PosaoId == posao.PosaoId).Include(s => s.Suradniks).Include(p => p.VrstaPosla).Include(p => p.Projekt).FirstOrDefaultAsync();
                return PartialView("~/Views/Posao/Get.cshtml", posao2);
            }
            catch (Exception exc)
            {
                return BadRequest("Pogreška tijekom spremanja.");
            }

        }

        /// <summary>
        /// Post metoda za brisanje posla s dinamičkim odzivom.
        /// </summary>
        /// <param name="PosaoId">ID posla koji se briše.</param>
        /// <returns>EmptyResult ili PartialView s informacijama o poslu ovisno o uspješnosti brisanja.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteDynamic(int PosaoId) {
       
        var responseMessage = new ActionResponseMessage(MessageType.Success,
            $"Posao sa šifrom {PosaoId} uspješno obrisan.");

        try {
            var posao = _context.Posaos.Find(PosaoId);
            _context.Remove(posao);
            _context.SaveChanges();
            
        }
        catch (Exception exc) {
            responseMessage = new ActionResponseMessage(MessageType.Error,
            $"Pogreška prilikom brisanja posla");
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(
        new { showMessage = responseMessage });
        
        return responseMessage.MessageType == MessageType.Success ?
        new EmptyResult() : await Get(PosaoId);
        }

        /// <summary>
        /// Dohvaća vrste posla prema zadanim kriterijima.
        /// </summary>
        /// <param name="term">Pojam za pretragu vrsti posla.</param>
        /// <returns>Kolekcija ID-Label parova koji predstavljaju vrste posla.</returns>
        public async Task<IEnumerable<IdLabel>> VrstaPosla(string term)
        {

            var query = _context.VrstaPoslas
            .Select(v => new IdLabel
            {
                Id = v.VrstaPoslaId,
                Label = v.NazivVrste
            })
            .Where(p => p.Label.Contains(term));

            var list = await query.OrderBy(l => l.Label)
                .Take(5)
                .ToListAsync();

            return list;
        }


    }


}

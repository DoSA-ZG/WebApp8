using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

using System.Text.Json;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;


namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje operacijama vezanim uz suradnike.
    /// </summary>
    public class SuradnikController : Controller
    {
        private readonly Rppp08Context _context;

        /// <summary>
        /// Konstruktor kontrolera s primarnim kontekstom baze podataka.
        /// </summary>
        public SuradnikController(Rppp08Context context)
        {
            _context = context;
        }


        // GET: Suradnik
        /// <summary>
        /// Prikazuje popis suradnika.
        /// </summary>
        /// <returns>View s popisom suradnika.</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Suradniks.Include(s => s.Posaos).Include(s => s.SuradnikProjekts).ThenInclude(s => s.Projekt).ToListAsync());
        }


        /// <summary>
        /// Priprema padajuće popise za dodavanje novog suradnika.
        /// </summary>
        /// <returns>Task rezultat.</returns>
        private async Task PrepareDropDownLists()
        {
            var suradnici = await _context.Suradniks
            .Select(s => new { s.SuradnikId, s.SuradnikProjekts }).ToListAsync();
            ViewBag.Suradnici = new SelectList(suradnici,
            "SuradnikId", "SuradnikProjekts");

        }

        /// <summary>
        /// Prikazuje formu za dodavanje novog suradnika.
        /// </summary>
        /// <returns>View s formom za dodavanje novog suradnika.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Post metoda za stvaranje novog suradnika.
        /// </summary>
        /// <param name="suradnik">Suradnik koji se dodaje.</param>
        /// <returns>Redirekcija na Index ili View s porukom o grešci.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Suradnik suradnik)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(suradnik);
                    _context.SaveChanges();

                    TempData[Constants.Message] =
                    $"Suradnik {suradnik.Ime} dodan.";
                    TempData[Constants.ErrorOccurred] = false;


                    return RedirectToAction("Index");
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty,
                    exc.CompleteExceptionMessage());

                    await PrepareDropDownLists();
                    return View("Create", suradnik);
                }
            }

            await PrepareDropDownLists();
            return View("Create", suradnik);


        }

        /// <summary>
        /// Post metoda za brisanje suradnika.
        /// </summary>
        /// <param name="SuradnikId">ID suradnika koji se briše.</param>
        /// <returns>Redirekcija na Index.</returns>
        [HttpPost]
        public IActionResult Delete(int SuradnikId)
        {

            var suradnik = _context.Suradniks.Find(SuradnikId);

            Console.WriteLine(suradnik);

            try
            {
                _context.Remove(suradnik);
                _context.SaveChanges();
                TempData[Constants.Message] =
                       $"Suradnik {suradnik.Ime} obrisan.";
                TempData[Constants.ErrorOccurred] = false;
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = $"Neuspjeh pri brisanju suradnika.";
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction("Index");

        }

        /// <summary>
        /// Prikazuje formu za uređivanje suradnika.
        /// </summary>
        /// <param name="id">ID suradnika koji se uređuje.</param>
        /// <returns>View s formom za uređivanje suradnika.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var suradnik = _context.Suradniks.Find(id);

            if (suradnik == null)
            {
                return NotFound("Ne postoji suradnik s id " + id);
            }

            await PrepareDropDownLists();
            return View(suradnik);
        }

        /// <summary>
        /// Prikazuje detalje o određenom suradniku.
        /// </summary>
        /// <param name="id">ID suradnika čiji se detalji prikazuju.</param>
        /// <param name="index">Indeks trenutnog suradnika.</param>
        /// <returns>View s detaljima o suradniku.</returns>
        [HttpGet]
        public async Task<IActionResult> Detail(int id, int index)
        {
            ViewBag.Index = index;
            ViewBag.SuradnikId = id;
            var suradnik = await _context.Suradniks.Where(s => s.SuradnikId == id).Include(s => s.SuradnikProjekts).ThenInclude(s => s.Projekt).Include(s => s.Posaos).ThenInclude(p => p.VrstaPosla).FirstOrDefaultAsync();

            if (suradnik == null)
            {
                return NotFound("Ne postoji suradnik s id " + id);
            }

            await SetPreviousAndNext(index);

            await PrepareDropDownLists();
            return View(suradnik);
        }

        /// <summary>
        /// Post metoda za ažuriranje suradnika.
        /// </summary>
        /// <param name="id">ID suradnika koji se ažurira.</param>
        /// <returns>Redirekcija na Index ili View s porukom o grešci.</returns>
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Update(int id)
        {
            var suradnik = await _context.Suradniks.Where(s => s.SuradnikId == id).FirstOrDefaultAsync();

            if (suradnik == null)
            {
                return NotFound("Ne postoji suradnik s id " + id);
            }

            if (await TryUpdateModelAsync<Suradnik>(suradnik, "", s => s.SuradnikId))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData[Constants.Message] = "Suradnik uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {

                    ModelState.AddModelError("", "Neuspjeh pri spremanju podataka.. ");
                }
            }

            await PrepareDropDownLists();
            return View(suradnik);
        }

        /// <summary>
        /// Post metoda za ažuriranje detalja suradnika.
        /// </summary>
        /// <param name="suradnik">Suradnik koji se ažurira.</param>
        /// <param name="index">Indeks trenutnog suradnika.</param>
        /// <returns>PartialView s ažuriranim detaljima o suradniku.</returns>
        [HttpPost]
        public async Task<IActionResult> EditDetail(Suradnik suradnik, int index)
        {

            ViewBag.index = index;
            await SetPreviousAndNext(index);

            if (!ModelState.IsValid)
            {
                await PrepareDropDownLists();
                return PartialView("~/Views/Suradnik/EditDetail.cshtml", suradnik);
            }


            var responseMessage = new ActionResponseMessage(MessageType.Success,
            $"Suradnik {suradnik.Ime} uspješno ažurirano.");
            try
            {
                _context.Update(suradnik);
                await _context.SaveChangesAsync();
                var suradnik2 = await _context.Suradniks
                    .Where(s => s.SuradnikId == suradnik.SuradnikId).Include(s => s.Posaos).FirstOrDefaultAsync();
                await PrepareDropDownLists();

                Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(
                new { showMessage = responseMessage });

                return PartialView("~/Views/Suradnik/EditDetail.cshtml", suradnik2);
            }
            catch (Exception exc)
            {
                responseMessage = new ActionResponseMessage(MessageType.Error,
                    $"Pogreška prilikom spremanja podataka");
                Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(
                    new { showMessage = responseMessage });

                return BadRequest("Pogreška tijekom spremanja.");
            }

        }

        /// <summary>
        /// Postavlja prethodnog i sljedećeg suradnika na temelju trenutnog položaja.
        /// </summary>
        /// <param name="position">Trenutni položaj suradnika.</param>
        private async Task SetPreviousAndNext(int position) { 

                    var query = _context.Suradniks;

                    ViewBag.Next = null;
                    ViewBag.Previous = null;

                    if (position > 0) {
                        ViewBag.Previous = await query.Skip(position - 1)
                        .Select(s => s.SuradnikId)
                        .FirstAsync();
                    }

                    if (position < await query.CountAsync() - 1) {
                        ViewBag.Next = await query.Skip(position + 1)
                        .Select(s => s.SuradnikId)
                        .FirstAsync();
                        
                    }

        }

    }

}

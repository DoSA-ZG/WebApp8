using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Models; 
using Microsoft.AspNetCore.Mvc.Rendering;

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers 
{
    /// <summary>
    /// Kontroler za sve akcije vezane uz kartice projekata.
    /// </summary>
    public class KarticaProjektaController : Controller
    {
        private readonly Rppp08Context _context;
        

        public KarticaProjektaController(Rppp08Context context)
        {
            _context = context;
        }
        

        /// <summary>
        /// Kontroler za sve akcije vezane uz kartice projekata.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            return View(await _context.KarticaProjekta.Include(kp => kp.Transakcijas).Include(kp => kp.Projekt).ToListAsync());
        }


        /// <summary>
        /// Priprema padajući izbornik projekata tako da dohvaća identifikatore i nazive projekata iz baze podataka,
        /// te popunjava 'ViewBag.Projekti' s popisom za korištenje u web aplikaciji.
        /// </summary>
        private async Task PrepareDropDownLists() {
            var projekti = await _context.Projekts
            .Select(p => new { p.ProjektId , p.NazivProjekta}).ToListAsync();
            ViewBag.Projekti = new SelectList(projekti,
            "ProjektId", "NazivProjekta");
            
            }

    

        /// <summary>
        /// Prikazuje formu za stvaranje nove kartice.
        /// </summary>
        /// <returns>Pogled za kreiranje kartice.</returns>
        /// <exception cref="Exception">Baca iznimku ako neuspješno pripremi padajući izbornik.</exception>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }
        
        /// <summary>
        /// Post metoda za stvaranje nove kartice projekta.
        /// </summary>
        /// <param name="kartica">Podaci o kartici projekta koji se dodaje.</param>
        /// <returns>
        /// Ako su podaci ispravni i uspješno se dodaje kartica, preusmjerava na akciju "Index".
        /// Ako se pojavi iznimka, prikazuje se poruka o grešci i vraća na akciju "Create" za ponovno unošenje podataka.
        /// </returns>
        /// <exception cref="Exception">Baca iznimku ako se neuspješno dodaje kartica projekta.</exception>
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(KarticaProjektum kartica)
        {   

            if (ModelState.IsValid) {
                try {
                _context.Add(kartica);
                _context.SaveChanges();

                TempData[Constants.Message] =
                $"Kartica {kartica.VirtualniIban} dodana.";
                TempData[Constants.ErrorOccurred] = false;
                
                
                return RedirectToAction("Index");
                } catch (Exception exc) {

                ModelState.AddModelError(string.Empty,
                exc.CompleteExceptionMessage());
               
                await PrepareDropDownLists();
                return View("Create", kartica); }
            }

            await PrepareDropDownLists();
            return View("Create", kartica);
            
         
        }

        /// <summary>
        /// Post metoda za brisanje kartice projekta.
        /// </summary>
        /// <param name="KarticaProjektaId">ID kartice projekta koja se briše.</param>
        /// <returns>
        /// Ako je brisanje uspješno, preusmjerava na akciju "Index" s porukom o uspješnom brisanju.
        /// Ako se pojavi iznimka pri brisanju, preusmjerava na akciju "Index" s porukom o neuspjehu brisanja.
        /// </returns>
        [HttpPost]
        public IActionResult Delete(int KarticaProjektaId) {

        var kartica = _context.KarticaProjekta.Include(k => k.Transakcijas).FirstOrDefault(k => k.KarticaProjektaId == KarticaProjektaId);;

        
        try {
        _context.Remove(kartica);
        _context.SaveChanges();
         TempData[Constants.Message] =
                $"Kartica {kartica.VirtualniIban} obrisana.";
                TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
        TempData[Constants.Message] = $"Neuspjeh pri brisanju kartice.";
         TempData[Constants.ErrorOccurred] = true;
        }
        return RedirectToAction("Index");
        
    }


    /// <summary>
    /// Prikazuje formu za uređivanje kartice projekta.
    /// </summary>
    /// <param name="id">ID kartice projekta koja se uređuje.</param>
    /// <returns>
    /// Ako kartica s traženim ID-om postoji, priprema dropdown liste i prikazuje formu za uređivanje.
    /// Ako kartica ne postoji, vraća poruku "Ne postoji kartica s ID-om" i status "NotFound".
    /// </returns>
    [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {   
            var kartica = _context.KarticaProjekta.Find(id);

            if (kartica == null) {
                return NotFound("Ne postoji kartica s id " + id);
            }

            await PrepareDropDownLists();
            return View(kartica);
        }

    /// <summary>
    /// Prikazuje detalje kartice projekta.
    /// </summary>
    /// <param name="id">ID kartice projekta za prikaz detalja.</param>
    /// <param name="index">Indeks kartice u listi za navigaciju između prethodne i sljedeće kartice.</param>
    /// <returns>
    /// Ako kartica s traženim ID-om postoji, postavlja potrebne ViewBag vrijednosti, priprema dropdown liste i prikazuje detalje kartice.
    /// Ako kartica ne postoji, vraća poruku "Ne postoji kartica s ID-om" i status "NotFound".
    /// </returns
    [HttpGet]
        public async Task<IActionResult> Detail(int id, int index)
        {   

            ViewBag.Index = index;
            ViewBag.KarticaProjektaId = id;
            var kartica = await _context.KarticaProjekta.Where(k => k.KarticaProjektaId == id).Include(k => k.Projekt).Include(k => k.Transakcijas).ThenInclude(t => t.VrstaTransakcije).FirstOrDefaultAsync();

            if (kartica == null) {
                return NotFound("Ne postoji kartica s id " + id);
            }
            
            await SetPreviousAndNext(index);

            await PrepareDropDownLists();
            return View(kartica);
        }


        /// <summary>
        /// Post metoda za ažuriranje kartice projekta.
        /// </summary>
        /// <param name="id">ID kartice projekta koja se ažurira.</param>
        /// <returns>
        /// Ako su podaci ispravni i uspješno se ažurira kartica, preusmjerava na akciju "Index" s porukom o uspješnom ažuriranju.
        /// Ako se pojavi iznimka pri ažuriranju, prikazuje se poruka o neuspjehu ažuriranja i ostaje na formi za uređivanje.
        /// Ako kartica ne postoji, vraća poruku "Ne postoji kartica s ID-om" i status "NotFound".
        /// </returns>
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Update(int id) {
            var kartica = await _context.KarticaProjekta.Where(k => k.KarticaProjektaId == id).FirstOrDefaultAsync();

            if (kartica == null) {
                return NotFound("Ne postoji kartica s id " + id);
            }

            if (await TryUpdateModelAsync<KarticaProjektum>(kartica, "", k => k.StanjeKartice, k => k.VirtualniIban, k => k.ProjektId)) {
                try {
                    await _context.SaveChangesAsync();
                    TempData[Constants.Message] = "Kartica uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction("Index");
                } catch (DbUpdateException ex) {
                
                    ModelState.AddModelError("", "Neuspjeh pri spremanju podataka.. ");
                }
            }
            
            await PrepareDropDownLists();
            return View(kartica);
        } 


        /// <summary>
        /// Post metoda za uređivanje detalja kartice projekta (gornji dio u master-detailu).
        /// </summary>
        /// <param name="kartica">Podaci o kartici projekta koji se uređuju.</param>
        /// <param name="index">Indeks za prikaz prethodne i sljedeće kartice.</param>
        /// <returns>
        /// Ako su podaci ispravni i uspješno se ažuriraju, vraća parcijalni pogled s ažuriranim podacima kartice.
        /// Ako se pojavi iznimka pri ažuriranju, vraća poruku o pogrešci i status "BadRequest".
        /// </returns>
         [HttpPost]
        public async Task<IActionResult> EditDetail(KarticaProjektum kartica, int index) {

             ViewBag.index = index;
            await SetPreviousAndNext(index);

           if (!ModelState.IsValid) {
                await PrepareDropDownLists();
                return PartialView("~/Views/KarticaProjekta/EditDetail.cshtml", kartica);
           }
          

            var responseMessage = new ActionResponseMessage(MessageType.Success,
            $"Kartica {kartica.VirtualniIban} uspješno ažurirana.");
            try {
                _context.Update(kartica);
                await _context.SaveChangesAsync();
                var kartica2 = await _context.KarticaProjekta
                    .Where(k => k.KarticaProjektaId == kartica.KarticaProjektaId).Include(k => k.Projekt).FirstOrDefaultAsync();
                await PrepareDropDownLists();
               
               


                Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(
                new { showMessage = responseMessage });
        
                return PartialView("~/Views/KarticaProjekta/EditDetail.cshtml", kartica2);
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
    /// Postavlja vrijednosti za prethodnu i sljedeću karticu na temelju trenutne pozicije.
    /// </summary>
    /// <param name="position">Trenutna pozicija kartice.</param
    private async Task SetPreviousAndNext(int position) { 

                    var query = _context.KarticaProjekta.Include(kp => kp.Transakcijas).Include(kp => kp.Projekt);

                    ViewBag.Next = null;
                    ViewBag.Previous = null;

                    if (position > 0) {
                        ViewBag.Previous = await query.Skip(position - 1)
                        .Select(k => k.KarticaProjektaId)
                        .FirstAsync();
                    }

                    if (position < await query.CountAsync() - 1) {
                        ViewBag.Next = await query.Skip(position + 1)
                        .Select(k => k.KarticaProjektaId)
                        .FirstAsync();
                        
                    }

                }


  

    }

}
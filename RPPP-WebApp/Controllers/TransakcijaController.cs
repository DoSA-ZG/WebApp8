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
    /// Kontroler za akcije povezane s entitetom Transakcija.
    /// </summary>
    public class TransakcijaController : Controller
    {
        private readonly Rppp08Context _context;
        

        public TransakcijaController(Rppp08Context context)
        {
            _context = context;
        }
        

        // GET: KarticaProjektum
        public async Task<IActionResult> Index()
        {
            return View(await _context.Transakcijas.Include(kp => kp.KarticaProjekta).Include(t => t.VrstaTransakcije).ToListAsync());
        }


        /// <summary>
        /// Popunjava padajuće liste za odabir kartica i vrsta transakcija.
        /// </summary>
        private async Task PrepareDropDownLists() {
            var kartice = await _context.KarticaProjekta
            .Select(p => new { p.KarticaProjektaId , p.VirtualniIban}).ToListAsync();
            ViewBag.Kartice = new SelectList(kartice,
            "KarticaProjektaId", "VirtualniIban");
            
            var vrsteTransakcije = await _context.VrstaTransakcijes
            .Select(p => new { p.VrstaTransakcijeId , p.NazivVrste}).ToListAsync();
            ViewBag.VrsteTransakcije = new SelectList(vrsteTransakcije,
            "VrstaTransakcijeId", "NazivVrste");
            
            }

    
        /// <summary>
        /// Prikazuje formu za stvaranje nove transakcije.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        /// <summary>
        /// Pohranjuje novu transakciju u bazu podataka i preusmjerava na popis transakcija.
        /// </summary>
        /// <param name="transakcija">Podaci nove transakcije za pohranu.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(Transakcija transakcija)
        {   

            if (ModelState.IsValid) {
                try {
                _context.Add(transakcija);
                _context.SaveChanges();

                TempData[Constants.Message] =
                $"Transakcija sa opisom '{transakcija.Opis}' dodana.";
                TempData[Constants.ErrorOccurred] = false;
                
                
                return RedirectToAction("Index");

                } catch (Exception exc) {

                ModelState.AddModelError(string.Empty,
                exc.CompleteExceptionMessage());
               
                await PrepareDropDownLists();
                return View("Create", transakcija); }
            }

            await PrepareDropDownLists();
            return View("Create", transakcija);
            
         
        }


        /// <summary>
        /// Dodaje novi redak transakcije povezan s određenom karticom projekta.
        /// </summary>
        /// <param name="transakcija">Podaci nove transakcije za dodavanje.</param>
        /// <param name="KarticaProjektaId">ID kartice projekta povezane s transakcijom.</param>
        [HttpPost]
        public async Task<IActionResult> AddRow(Transakcija transakcija, int KarticaProjektaId)
        {   
        
            ViewBag.KarticaProjektaId = KarticaProjektaId;
             if (!ModelState.IsValid)
             {  
                
                if (transakcija.VrstaTransakcijeId == null) {
                
                    ModelState.AddModelError("VrstaTransakcijeId", "Vrsta transakcije mora biti odabrana.");
                
                } else {
                     transakcija.VrstaTransakcije = _context.VrstaTransakcijes.Find(transakcija.VrstaTransakcijeId);
                     ViewBag.NazivVrsteTransakcije = _context.VrstaTransakcijes.Find(transakcija.VrstaTransakcijeId).NazivVrste;

                }


                await PrepareDropDownLists();
                return PartialView("GetAddRow", transakcija);
            }

            if (ModelState.IsValid) {

                if (transakcija.VrstaTransakcijeId == null) {
                
                    ModelState.AddModelError("VrstaTransakcijeId", "Vrsta transakcije mora biti odabrana.");
                    return PartialView("GetAddRow", transakcija);
                    }

                try {
                _context.Add(transakcija);
                _context.SaveChanges();

                var transakcija2 = await _context.Transakcijas
            .Where(t => t.TransakcijaId == transakcija.TransakcijaId).Include(t => t.VrstaTransakcije).FirstOrDefaultAsync();
                return PartialView("Get",transakcija);

                } catch (Exception exc) {
                
                    
                await PrepareDropDownLists();
                return PartialView("GetAddRow", transakcija);
                
                }
            }


            await PrepareDropDownLists();
            return BadRequest("ERORR OCCURED WITH VALIDATION");
            
         
        }

        /// <summary>
        /// Briše transakciju s određenim ID-om.
        /// </summary>
        /// <param name="TransakcijaId">ID transakcije koju treba obrisati.</param>
        [HttpPost] 
        public IActionResult Delete(int TransakcijaId) {

        var transakcija = _context.Transakcijas.Find(TransakcijaId);

        
        try {
        _context.Remove(transakcija);
        _context.SaveChanges();
         TempData[Constants.Message] =
                $"Transakcija sa opisom '{transakcija.Opis}' obrisana.";
                TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
        TempData[Constants.Message] = $"Neuspjeh pri brisanju transakcije.";
         TempData[Constants.ErrorOccurred] = true;
        }
        return RedirectToAction("Index");
        
    }
        
        /// <summary>
        /// Odustajanje od dodavanja nove transakcije, rezultat je zatvaranje redka za dodavanje.
        /// </summary>
        /// <param name="TransakcijaId"></param>
        /// <returns>Prazan rezulat koji signalizira "uspješno" brisanje.</returns>
        [HttpDelete]
        public async Task<IActionResult> QuitAddRow(int TransakcijaId) { 

            return new EmptyResult();

        }

        /// <summary>
        /// HTTP DELETE akcija za brisanje transakcije prema njezinoj šifri. Koristi se za dinamičko brisanje retka.
        /// </summary>
        /// <param name="TransakcijaId">Šifra transakcije koja se briše.</param>
        /// <returns>HTTP odgovor koji sadrži informaciju o uspješnosti brisanja.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteDynamic(int TransakcijaId) {


        var responseMessage = new ActionResponseMessage(MessageType.Success,
            $"Transakcija sa šifrom {TransakcijaId} uspješno obrisana.");

        try {
            var transakcija = _context.Transakcijas.Find(TransakcijaId);
            _context.Remove(transakcija);
            _context.SaveChanges();
            responseMessage = new ActionResponseMessage(MessageType.Success,
            $"Transakcija sa šifrom {TransakcijaId} uspješno obrisana.");

            
        }
        catch (Exception exc) {
            responseMessage = new ActionResponseMessage(MessageType.Error,
            $"Pogreška prilikom brisanja transakcije");
        }

        Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(
        new { showMessage = responseMessage });
        
        return responseMessage.MessageType == MessageType.Success ?
        new EmptyResult() : await Get(TransakcijaId);
    }


    /// <summary>
/// HTTP GET akcija za prikaz forme za uređivanje transakcije prema njezinoj šifri.
/// </summary>
/// <param name="id">Šifra transakcije koja se uređuje.</param>
/// <returns>
/// Ako je transakcija pronađena, prikazuje se forma za uređivanje s popunjenim podacima.
/// Ako transakcija ne postoji, vraća se HTTP NotFound s odgovarajućom porukom.
/// </returns>
    [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {   
            var transakcija = _context.Transakcijas.Find(id);

            if (transakcija == null) {
                return NotFound("Ne postoji transakcija s id " + id);
            }

            await PrepareDropDownLists();
            return View(transakcija);
        }


        /// <summary>
        /// HTTP GET akcija za prikaz forme za uređivanje pojedinačne transakcije (redak) prema njezinoj šifri.
        /// </summary>
        /// <param name="id">Šifra transakcije koja se uređuje.</param>
        /// <returns>
        /// Ako je transakcija pronađena, prikazuje se forma za uređivanje s popunjenim podacima (koristi se PartialView).
        /// Ako transakcija ne postoji, vraća se HTTP NotFound s odgovarajućom porukom.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> EditRow(int id)
        {   
            var transakcija = await _context.Transakcijas.Include(t => t.VrstaTransakcije).Where(t => t.TransakcijaId == id).FirstOrDefaultAsync();


            if (transakcija == null) {
                return NotFound("Ne postoji transakcija s id " + id);
            }

            await PrepareDropDownLists();
            return PartialView(transakcija);
        }

        /// <summary>
        /// HTTP GET akcija za prikaz forme za dodavanje novog reda transakcije na određenu karticu projekta.
        /// </summary>
        /// <param name="KarticaProjektaId">Šifra kartice projekta za koju se dodaje transakcija.</param>
        /// <returns>
        /// Prikazuje formu za dodavanje novog reda transakcije na određenu karticu projekta (koristi se PartialView).
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAddRow(int KarticaProjektaId)
        {   
            
            ViewBag.KarticaProjektaId = KarticaProjektaId;
            await PrepareDropDownLists();
            return PartialView();
        }



        /// <summary>
        /// HTTP POST akcija za ažuriranje postojeće transakcije.
        /// </summary>
        /// <param name="id">Šifra transakcije koja se ažurira.</param>
        /// <returns>
        /// Ako je ažuriranje uspješno, preusmjerava na akciju "Index" s porukom o uspješnom ažuriranju.
        /// Ako ažuriranje nije uspjelo, prikazuje formu za uređivanje transakcije s odgovarajućim porukama o greškama.
        /// </returns>
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Update(int id) {
            var transakcija = await _context.Transakcijas.Where(t => t.TransakcijaId == id).FirstOrDefaultAsync();

            if (transakcija == null) {
                return NotFound("Ne postoji transakcija s id " + id);
            }

            if (await TryUpdateModelAsync<Transakcija>(transakcija, "", t => t.DatumTransakcije, t => t.Iznos, t => t.SubjektIban,
                    t => t.PrimateljIban, t => t.Opis, t => t.KarticaProjektaId, t  => t.VrstaTransakcijeId
                )) {
                try {
                    await _context.SaveChangesAsync();
                    TempData[Constants.Message] = "Transakcija uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction("Index");
                } catch (DbUpdateException ex) {
                
                    ModelState.AddModelError("", "Neuspjeh pri spremanju podataka.. ");
                }
            }
            
            await PrepareDropDownLists();
            return View(transakcija);
        } 


        /// <summary>
        /// HTTP POST akcija za ažuriranje podataka o transakciji iz redaka tablice.
        /// </summary>
        /// <param name="transakcija">Objekt transakcije koji se ažurira.</param>
        /// <returns>
        /// Ako su podaci o transakciji valjani i uspješno ažurirani, vraća djelomični pogled sa ažuriranim podacima.
        /// Ako podaci nisu valjani, vraća djelomični pogled s odgovarajućim porukama o greškama.
        /// Ako se dogodi pogreška tijekom ažuriranja u bazi podataka, vraća BadRequest s odgovarajućom porukom.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> EditRow(Transakcija transakcija) {
           
            if (!ModelState.IsValid) {

                 if (transakcija.VrstaTransakcijeId == null) {
                
                        ModelState.AddModelError("VrstaTransakcijeId", "Vrsta transakcije mora biti odabrana.");
                
                 }

                await PrepareDropDownLists();
                var transakcija2 = await _context.Transakcijas.Include(t => t.VrstaTransakcije).Where(t => t.TransakcijaId == transakcija.TransakcijaId).FirstOrDefaultAsync();
               
                return PartialView(transakcija2);
            }
          
            if (transakcija.VrstaTransakcijeId == null) {
                
                 var transakcija2 = await _context.Transakcijas.Include(t => t.VrstaTransakcije).Where(t => t.TransakcijaId == transakcija.TransakcijaId).FirstOrDefaultAsync();
                ModelState.AddModelError("VrstaTransakcijeId", "Vrsta transakcije mora biti odabrana.");
                await PrepareDropDownLists();
                return PartialView(transakcija2);
            }


            try {
                _context.Update(transakcija);
                await _context.SaveChangesAsync();
                var transakcija2 = await _context.Transakcijas
                    .Where(t => t.TransakcijaId == transakcija.TransakcijaId).Include(t => t.VrstaTransakcije).FirstOrDefaultAsync();
                return PartialView("~/Views/Transakcija/Get.cshtml", transakcija2);
            }
            catch (Exception exc)
            {
                return BadRequest("Pogreška tijekom spremanja.");
            }

        } 


        /// <summary>
        /// HTTP GET akcija za dohvat retka transakcije s određenim ID-om.
        /// </summary>
        /// <param name="id">ID transakcije koju želite dohvatiti.</param>
        /// <returns>
        /// Ako transakcija s određenim ID-om postoji, vraća djelomični pogled s detaljima transakcije.
        /// Ako transakcija s tim ID-om ne postoji, vraća NotFound rezultat s odgovarajućom porukom o grešci.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id) {
        var transakcija = await _context.Transakcijas
            .Where(t => t.TransakcijaId == id).Include(t => t.VrstaTransakcije).FirstOrDefaultAsync();

            if (transakcija != null)
            return PartialView(transakcija);
            else
            return NotFound($"Neispravan id transakcije: {id}");
        }

        /// <summary>
        /// HTTP GET akcija za pretragu vrsta transakcija prema unesenom pojmu. Služi za autocomplete.
        /// </summary>
        /// <param name="term">Pojam za pretragu vrsta transakcija prema nazivu.</param>
        /// <returns>
        /// IEnumerable objekt koji sadrži listu IdLabel objekata koji predstavljaju vrste transakcija koje odgovaraju unesenom pojmu.
        /// Svaki IdLabel objekt sadrži ID i naziv vrste transakcije.
        /// Ograničava se na prvih 5 rezultata i sortira ih po nazivu.
        /// </returns>
        public async Task<IEnumerable<IdLabel>> VrstaTransakcije(string term) {

            var query = _context.VrstaTransakcijes
            .Select(v => new IdLabel {
                Id = v.VrstaTransakcijeId,
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
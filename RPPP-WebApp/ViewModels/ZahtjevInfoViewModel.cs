using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying detailed information about a Zahtjev, including associated Zadatci.
    /// </summary>
    public class ZahtjevInfoViewModel
    {
        public int ZahtjevId { get; set; }
        public int ProjektId { get; set; }
        public int VrstaZahtjevaId { get; set; }

        [Required(ErrorMessage = "Potrebno je odabrati oznaku zahtjeva")]
        public string Oznaka { get; set; }

        [Required(ErrorMessage = "Potrebno je odabrati naziv zahtjeva")]
        public string NazivZahtjeva { get; set; }

        [Required(ErrorMessage = "Potrebno je odabrati vrstu zahtjeva")]
        public string NazivVrsteZahtjeva { get; set; }

        [Required(ErrorMessage = "Potrebno je odabrati projekt")]
        public string NazivProjekta { get; set; }

        [Required(ErrorMessage = "Potrebno je odabrati prioritet")]
        public string Prioritet { get; set; }

        public IEnumerable<ZadatakViewModel> Zadatci { get; set; }

        public ZahtjevInfoViewModel()
        {
            this.Zadatci = new List<ZadatakViewModel>();
        }
    }
}

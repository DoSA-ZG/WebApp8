using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying information about a Zahtjev.
    /// </summary>
    public class ZahtjevViewModel
    {
        public int ZahtjevId { get; set; }
        public string Oznaka { get; set; }
        public string NazivZahtjeva { get; set; }
        public string NazivVrsteZahtjeva { get; set; }
        public string NazivProjekta { get; set; }
        public string Prioritet { get; set; }
        public ICollection<Zadatak> Zadatci { get; set; }
    }
}

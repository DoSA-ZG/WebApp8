using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikaz informacija o zadatku.
    /// </summary>
    public class ZadatakViewModel
    {
        /// <summary>
        /// Identifikator zadatka.
        /// </summary>
        public int ZadatakId { get; set; }

        /// <summary>
        /// Opis zadatka.
        /// </summary>
        public string Opis { get; set; }

        /// <summary>
        /// Status zadatka.
        /// </summary>
        public string StatusZadatka { get; set; }

        /// <summary>
        /// Oznaka zahtjeva.
        /// </summary>
        public string OznakaZahtjeva { get; set; }

        /// <summary>
        /// Identifikator statusa zadatka.
        /// </summary>
        public int StatusZadatkaId { get; set; }
    }
}

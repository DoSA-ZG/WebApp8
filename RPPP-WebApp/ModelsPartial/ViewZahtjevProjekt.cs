namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Prikaz informacija o projektu u kontekstu zahtjeva.
    /// </summary>
    public class ViewZahtjevProjekt
    {
        /// <summary>
        /// Identifikator projekta.
        /// </summary>
        public int ProjektId { get; set; }

        /// <summary>
        /// Naziv projekta.
        /// </summary>
        public string NazivProjekta { get; set; }

        /// <summary>
        /// Kratica projekta.
        /// </summary>
        public string KraticaProjekta { get; set; }
    }
}

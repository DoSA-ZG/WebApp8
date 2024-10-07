using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Klasa koja predstavlja model VrstaZahtjeva.
    /// </summary>
    public partial class VrstaZahtjeva
    {
        /// <summary>
        /// Identifikator vrste zahtjeva.
        /// </summary>
        public int VrstaZahtjevaId { get; set; }

        /// <summary>
        /// Naziv vrste zahtjeva.
        /// </summary>
        public string NazivVrsteZahtjeva { get; set; }

        /// <summary>
        /// Popis zahtjeva povezanih s ovom vrstom zahtjeva.
        /// </summary>
        public virtual ICollection<Zahtjev> Zahtjevs { get; set; } = new List<Zahtjev>();
    }
}

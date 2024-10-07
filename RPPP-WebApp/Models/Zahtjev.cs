using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Klasa koja predstavlja model Zahtjev.
    /// </summary>
    public partial class Zahtjev
    {
        /// <summary>
        /// Identifikator zahtjeva.
        /// </summary>
        public int ZahtjevId { get; set; }

        /// <summary>
        /// Identifikator vrste zahtjeva (obavezno polje).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Vrsta zahtjeva je obavezna!")]
        public int VrstaZahtjevaId { get; set; }

        /// <summary>
        /// Identifikator projekta (obavezno polje).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Projekt je obavezan!")]
        public int ProjektId { get; set; }

        /// <summary>
        /// Naziv zahtjeva (obavezno polje).
        /// </summary>
        [Required(ErrorMessage = "Naziv zahtjeva je obavezan!")]
        public string NazivZahtjeva { get; set; }

        /// <summary>
        /// Prioritet zahtjeva (obavezno polje).
        /// </summary>
        [Required(ErrorMessage = "Prioritet je obavezan!")]
        public string Prioritet { get; set; } // mora biti padajuća lista!

        /// <summary>
        /// Oznaka zahtjeva (obavezno polje).
        /// </summary>
        [Required(ErrorMessage = "Oznaka zahtjeva je obavezna!")]
        // [RegularExpression(@"^Z-\d{4}-\d{4}$", ErrorMessage="Format : Z-XXXX-XXXX")]
        public string Oznaka { get; set; } // ZH10 itd

        /// <summary>
        /// Projekat povezan s ovim zahtjevom.
        /// </summary>
        public virtual Projekt Projekt { get; set; }

        /// <summary>
        /// Vrsta zahtjeva povezana s ovim zahtjevom.
        /// </summary>
        public virtual VrstaZahtjeva VrstaZahtjeva { get; set; } // mora biti padajuća lista!

        /// <summary>
        /// Popis zadataka povezanih s ovim zahtjevom.
        /// </summary>
        public virtual ICollection<Zadatak> Zadataks { get; } = new List<Zadatak>();
    }
}

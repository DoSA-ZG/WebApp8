using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Predstavlja model kartice projekta.
    /// </summary>
    public partial class KarticaProjektum
    {
        /// <summary>
        /// Jedinstveni identifikator kartice projekta.
        /// </summary>
        public int KarticaProjektaId { get; set; }

        /// <summary>
        /// ID povezanog projekta.
        /// </summary>
        [Display(Name = "Projekt")]
        [Range(1, int.MaxValue, ErrorMessage = "Morate odabrati projekt.")]
        public int? ProjektId { get; set; }

        /// <summary>
        /// Stanje na kartici projekta.
        /// </summary>
        [Display(Name = "Stanje Kartice")]
        [Required(ErrorMessage = "Stanje je obavezno.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stanje kartice mora biti veće od 0.")]
        public int? StanjeKartice { get; set; }

        /// <summary>
        /// Virtualni IBAN kartice projekta.
        /// </summary>
        [Display(Name = "Virtualni IBAN")]
        [Required(ErrorMessage = "IBAN je obavezan.")]
        [RegularExpression(@"^[A-Z]{2}\d+$", ErrorMessage = "IBAN nije u ispravnom formatu")]
        public string VirtualniIban { get; set; }

        /// <summary>
        /// Povezani projekt za karticu projekta.
        /// </summary>
        public virtual Projekt Projekt { get; set; }

        /// <summary>
        /// Kolekcija transakcija povezanih s karticom projekta.
        /// </summary>
        public virtual ICollection<Transakcija> Transakcijas { get; set; } = new List<Transakcija>();
    }
}

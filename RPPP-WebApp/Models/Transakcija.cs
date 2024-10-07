using System;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Predstavlja model transakcije.
    /// </summary>
    public partial class Transakcija
    {
        /// <summary>
        /// Jedinstveni identifikator transakcije.
        /// </summary>
        public int TransakcijaId { get; set; }

        /// <summary>
        /// Datum transakcije.
        /// </summary>
        [Display(Name = "Datum Transakcije")]
        [Required(ErrorMessage = "Datum je obavezan.")]
        public DateTime? DatumTransakcije { get; set; }

        /// <summary>
        /// Iznos transakcije.
        /// </summary>
        [Display(Name = "Iznos")]
        [Required(ErrorMessage = "Iznos je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Iznos mora biti > 1.")]
        public decimal? Iznos { get; set; }

        /// <summary>
        /// IBAN subjekta transakcije.
        /// </summary>
        [Display(Name = "Subjektov IBAN")]
        [Required(ErrorMessage = "Subjektov IBAN je obavezan.")]
        [RegularExpression(@"^[A-Z]{2}\d+$", ErrorMessage = "IBAN nije u ispravnom formatu")]
        public string SubjektIban { get; set; }

        /// <summary>
        /// IBAN primatelja transakcije.
        /// </summary>
        [Display(Name = "Primateljov IBAN")]
        [Required(ErrorMessage = "Primateljov IBAN je obavezan.")]
        [RegularExpression(@"^[A-Z]{2}\d+$", ErrorMessage = "IBAN nije u ispravnom formatu")]
        public string PrimateljIban { get; set; }

        /// <summary>
        /// Opis transakcije.
        /// </summary>
        [Required(ErrorMessage = "Opis je obavezan.")]
        public string Opis { get; set; }

        /// <summary>
        /// ID povezane kartice projekta.
        /// </summary>
        [Display(Name = "KarticaProjekta")]
        [Range(1, int.MaxValue, ErrorMessage = "Morate odabrati karticu.")]
        public int? KarticaProjektaId { get; set; }

        /// <summary>
        /// ID povezane vrste transakcije.
        /// </summary>
        [Display(Name = "Vrsta Transakcije")]
        [Range(1, int.MaxValue, ErrorMessage = "Morate odabrati vrstu transakcije.")]
        public int? VrstaTransakcijeId { get; set; }

        /// <summary>
        /// Povezana kartica projekta za transakciju.
        /// </summary>
        public virtual KarticaProjektum KarticaProjekta { get; set; }

        /// <summary>
        /// Povezana vrsta transakcije za transakciju.
        /// </summary>
        public virtual VrstaTransakcije VrstaTransakcije { get; set; }
    }
}

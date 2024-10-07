using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Klasa koja predstavlja model Zadatak.
    /// </summary>
    public partial class Zadatak
    {
        /// <summary>
        /// Identifikator zadatka.
        /// </summary>
        public int ZadatakId { get; set; }

        /// <summary>
        /// Opis zadatka (obavezno polje).
        /// </summary>
        [Required(ErrorMessage = "Opis zadatka je obavezan!")]
        public string Opis { get; set; }

        /// <summary>
        /// Identifikator statusa zadatka (obavezno polje).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Status zadatka je obavezan!")]
        public int StatusZadatkaId { get; set; }

        /// <summary>
        /// Identifikator zahtjeva (opcionalno polje).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Oznaka zahtjeva je obavezna!")]
        public int? ZahtjevId { get; set; }

        /// <summary>
        /// Status zadatka povezan s ovim zadatkom.
        /// </summary>
        public virtual StatusZadatka StatusZadatka { get; set; }

        /// <summary>
        /// Popis suradnika povezanih s ovim zadatkom.
        /// </summary>
        public virtual ICollection<ZadatakSuradnik> ZadatakSuradniks { get; } = new List<ZadatakSuradnik>();

        /// <summary>
        /// Zahtjev povezan s ovim zadatkom (opcionalno).
        /// </summary>
        public virtual Zahtjev Zahtjev { get; set; }
    }
}

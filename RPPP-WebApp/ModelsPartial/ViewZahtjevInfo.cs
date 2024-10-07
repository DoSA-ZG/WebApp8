using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Models
{
    /// <summary>
    /// Prikaz informacija o zahtjevu.
    /// </summary>
    public class ViewZahtjevInfo
    {
        /// <summary>
        /// Identifikator zahtjeva.
        /// </summary>
        public int ZahtjevId { get; set; }

        /// <summary>
        /// Naziv zahtjeva.
        /// </summary>
        // [Required(ErrorMessage = "Naziv zahtjeva je obavezan!")]
        public string NazivZahtjeva { get; set; }

        /// <summary>
        /// Prioritet zahtjeva.
        /// </summary>
        // [Required(ErrorMessage = "Prioritet je obavezan!")]
        public string Prioritet { get; set; }

        /// <summary>
        /// Oznaka zahtjeva.
        /// </summary>
        // [Required(ErrorMessage = "Oznaka zahtjeva je obavezna!")]
        public string Oznaka { get; set; }

        /// <summary>
        /// Identifikator vrste zahtjeva.
        /// </summary>
        // [Range(1, int.MaxValue, ErrorMessage = "Vrsta zahtjeva je obavezna!")]
        public int VrstaZahtjevaId { get; set; }

        /// <summary>
        /// Naziv vrste zahtjeva.
        /// </summary>
        public string NazivVrsteZahtjeva { get; set; }

        /// <summary>
        /// Identifikator projekta.
        /// </summary>
        // [Range(1, int.MaxValue, ErrorMessage = "Naziv projekta je obavezan!")]
        public int ProjektId { get; set; }

        /// <summary>
        /// Naziv projekta.
        /// </summary>
        public string NazivProjekta { get; set; }

        /// <summary>
        /// Položaj.
        /// </summary>
        [NotMapped]
        public int Position { get; set; }
    }
}

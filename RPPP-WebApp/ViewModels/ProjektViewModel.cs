using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektViewModel
    {
        public int ProjektId { get; set; }

        public DateTime? PlaniraniPocetak { get; set; }

        public DateTime? StvarniPocetak { get; set; }

        public DateTime? PlaniraniZavrsetak { get; set; }

        public DateTime? StvarniZavrsetak { get; set; }

        public string NazivProjekta { get; set; }

        public string KraticaProjekta { get; set; }

        public string OpisProjekta { get; set; }

        public int? VrstaProjektaId { get; set; }

        public virtual ICollection<Dokumentacija> Dokumentacijas { get; } = new List<Dokumentacija>();

        public virtual ICollection<KarticaProjektum> KarticaProjekta { get; } = new List<KarticaProjektum>();

        public virtual ICollection<PartnerProjekt> PartnerProjekts { get; } = new List<PartnerProjekt>();

        public virtual ICollection<Posao> Posaos { get; } = new List<Posao>();

        public virtual ICollection<SuradnikProjekt> SuradnikProjekts { get; } = new List<SuradnikProjekt>();

        public virtual VrstaProjektum VrstaProjekta { get; set; }

        public virtual ICollection<Zahtjev> Zahtjevs { get; } = new List<Zahtjev>();

    }
}

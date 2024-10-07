using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Models;

public partial class Projekt
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProjektId { get; set; }

    public DateTime? PlaniraniPocetak { get; set; }

    public DateTime? PlaniraniZavrsetak { get; set; }

    public DateTime? StvarniPocetak { get; set; }

    public DateTime? StvarniZavrsetak { get; set; }

    public string NazivProjekta { get; set; }

    public string KraticaProjekta { get; set; }

    public string OpisProjekta { get; set; }

    public int? VrstaProjektaId { get; set; }

    public virtual ICollection<Dokumentacija> Dokumentacijas { get; set; } = new List<Dokumentacija>();

    public virtual ICollection<KarticaProjektum> KarticaProjekta { get; set; } = new List<KarticaProjektum>();

    public virtual ICollection<PartnerProjekt> PartnerProjekts { get; set; } = new List<PartnerProjekt>();

    public virtual ICollection<Posao> Posaos { get; set; } = new List<Posao>();

    public virtual ICollection<SuradnikProjekt> SuradnikProjekts { get; set; } = new List<SuradnikProjekt>();

    public virtual VrstaProjektum VrstaProjekta { get; set; }

    public virtual ICollection<Zahtjev> Zahtjevs { get; set; } = new List<Zahtjev>();


}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models;

public partial class Dokumentacija
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DokumentacijaId { get; set; }

    public string NazivDokumentacije { get; set; }

    public int? ProjektId { get; set; }

    public int? VrstaDokumentacijeId { get; set; }

    public virtual Projekt Projekt { get; set; }

    public virtual VrstaDokumentacije VrstaDokumentacije { get; set; }
}

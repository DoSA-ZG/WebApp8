using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class VrstaDokumentacije
{
    public int VrstaDokumentacijeId { get; set; }

    public string NazivVrsteDokumentacije { get; set; }

    public virtual ICollection<Dokumentacija> Dokumentacijas { get; set; } = new List<Dokumentacija>();
}

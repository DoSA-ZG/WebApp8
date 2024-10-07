using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class VrstaProjektum
{
    public int VrstaProjektaId { get; set; }

    public string NazivVrsteProjekta { get; set; }

    public virtual ICollection<Projekt> Projekts { get; set; } = new List<Projekt>();
}

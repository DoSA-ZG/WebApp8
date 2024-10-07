using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class VrstaPosla
{
    public int VrstaPoslaId { get; set; }

    public string NazivVrste { get; set; }

    public virtual ICollection<Posao> Posaos { get; set; } = new List<Posao>();
}

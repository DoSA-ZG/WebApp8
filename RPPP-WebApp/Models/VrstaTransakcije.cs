using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class VrstaTransakcije
{
    public int VrstaTransakcijeId { get; set; }

    public string NazivVrste { get; set; }

    public virtual ICollection<Transakcija> Transakcijas { get; set; } = new List<Transakcija>();
}

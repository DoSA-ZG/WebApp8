using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class StatusZadatka
{
    public int StatusZadatkaId { get; set; }

    public string NazivStatusaZadatka { get; set; }

    public virtual ICollection<Zadatak> Zadataks { get; set; } = new List<Zadatak>();
}

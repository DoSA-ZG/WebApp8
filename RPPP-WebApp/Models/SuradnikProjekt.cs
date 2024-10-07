using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class SuradnikProjekt
{
    public int SuradnikProjektId { get; set; }

    public int? SuradnikId { get; set; }

    public int? ProjektId { get; set; }

    public string UlogaSuradnika { get; set; }

    public virtual Projekt Projekt { get; set; }

    public virtual Suradnik Suradnik { get; set; }
}

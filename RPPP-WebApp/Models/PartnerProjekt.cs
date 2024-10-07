using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class PartnerProjekt
{
    public int PartnerProjektId { get; set; }

    public int? PartnerId { get; set; }

    public int? ProjektId { get; set; }

    public string UlogaPartnera { get; set; }

    public virtual Partner Partner { get; set; }

    public virtual Projekt Projekt { get; set; }
}

using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Partner
{
    public int PartnerId { get; set; }

    public string Naziv { get; set; }

    public string Oib { get; set; }

    public string Iban { get; set; }

    public string Email { get; set; }

    public string Url { get; set; }

    public string Ime { get; set; }

    public string Prezime { get; set; }

    public virtual ICollection<PartnerProjekt> PartnerProjekts { get; set; } = new List<PartnerProjekt>();
}

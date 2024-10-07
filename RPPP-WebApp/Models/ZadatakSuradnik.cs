using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class ZadatakSuradnik
{
    public int ZadatakId { get; set; }

    public int SuradnikId { get; set; }

    public int? UtrosenoVrijeme { get; set; }

    public virtual Suradnik Suradnik { get; set; }

    public virtual Zadatak Zadatak { get; set; }
}

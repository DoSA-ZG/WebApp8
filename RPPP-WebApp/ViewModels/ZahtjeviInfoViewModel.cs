using System.Collections.Generic;
using RPPP_WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a list of ViewZahtjevInfo objects along with paging information and a filter.
    /// </summary>
    public class ZahtjeviInfoViewModel
    {
        public IEnumerable<ViewZahtjevInfo> Zahtjevi { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public ZahtjevFilter Filter { get; set; }
    }
}

using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a list of Zahtjevs along with paging information.
    /// </summary>
    public class ZahtjeviViewModel
    {
        public IEnumerable<ZahtjevViewModel> Zahtjevi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}

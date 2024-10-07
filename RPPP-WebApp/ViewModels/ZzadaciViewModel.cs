using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ZzadaciViewModel
    {
        public IEnumerable<Zadatak> Zadaci { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}

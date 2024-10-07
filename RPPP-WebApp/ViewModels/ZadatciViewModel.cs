using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikaz zadatka.
    /// </summary>
    public class ZadatciViewModel
    {
        /// <summary>
        /// Kolekcija zadatka.
        /// </summary>
        public IEnumerable<ZadatakViewModel> Zadatci { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}

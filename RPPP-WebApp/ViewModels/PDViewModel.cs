using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PDViewModel
    {
        public List<Projekt> ProjektData { get; set; }
        public List<Dokumentacija> DokumentacijaData { get; set; }

        public List<VrstaProjektum> VrstaProjektaData {  get; set; }
    }
}

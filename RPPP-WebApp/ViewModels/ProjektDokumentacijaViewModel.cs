using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
	public class ProjektDokumentacijaViewModel
	{
		public Projekt ProjektData { get; set; }
		public List<Dokumentacija> DokumentacijaData { get; set; }

		public List<VrstaDokumentacije> VrstaDokumentacije { get; set; }

        public Dokumentacija NewDokumentacija { get; set; }
    }
}


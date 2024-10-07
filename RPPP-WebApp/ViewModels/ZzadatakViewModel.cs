using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
	public class ZzadatakViewModel
	{
		public Zadatak zadatak {  get; set; }	

		public IEnumerable<ZsuradnikViewModel> zsuradnikViewModels { get; set; }

		public ZzadatakViewModel() 
		{
			this.zsuradnikViewModels = new List<ZsuradnikViewModel>();
		}	

    }
}

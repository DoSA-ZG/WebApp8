using RPPP_WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ZsuradnikViewModel
    {
        public Suradnik Suradnik { get; set; }
        public List<Zadatak> AvailableZadaci { get; set; }
        public int[] SelectedZadaci { get; set; }
    }
}

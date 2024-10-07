using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels
{
    public class AutoCompleteZsuradnik
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("Ime")]
        public string Ime { get; set; }

        [JsonPropertyName("Prezime")]
        public string Prezime { get; set; }

        [JsonPropertyName("Email")] 
        public string Email { get; set; }

        [JsonPropertyName("BrojMobitela")] 
        public string BrojMobitela { get; set; }    
        
        public AutoCompleteZsuradnik() { }
        public AutoCompleteZsuradnik(int id, string ime, string label, string prezime, string email, string brojMobitela)
        {
            Id = id;
            Label = label;
            Ime = ime;
            Prezime = prezime;
            Email = email;
            BrojMobitela = brojMobitela;
        }
    }
}

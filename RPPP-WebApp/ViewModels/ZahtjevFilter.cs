using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Filtriranje zahtjeva.
    /// </summary>
    public class ZahtjevFilter : IPageFilter
    {
        /// <summary>
        /// ID projekta.
        /// </summary>
        public int? ProjektId { get; set; }

        /// <summary>
        /// ID vrste zahtjeva.
        /// </summary>
        public int? VrstaZahtjevaId { get; set; }

        /// <summary>
        /// Naziv projekta.
        /// </summary>
        public string NazivProjekta { get; set; }

        /// <summary>
        /// Naziv vrste zahtjeva.
        /// </summary>
        public string NazivVrsteZahtjeva { get; set; }

        /// <summary>
        /// Oznaka zahtjeva.
        /// </summary>
        public string Oznaka { get; set; }

        /// <summary>
        /// Prioritet zahtjeva.
        /// </summary>
        public string Prioritet { get; set; }

        /// <summary>
        /// Provjerava je li filter prazan.
        /// </summary>
        /// <returns>True ako je filter prazan, inače false.</returns>
        public bool IsEmpty()
        {
            return !ProjektId.HasValue && !VrstaZahtjevaId.HasValue;
        }

        /// <summary>
        /// Pretvara filter u string reprezentaciju.
        /// </summary>
        /// <returns>String reprezentacija filtera.</returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", ProjektId, VrstaZahtjevaId);
        }

        /// <summary>
        /// Stvara instancu filtera iz string reprezentacije.
        /// </summary>
        /// <param name="s">String reprezentacija filtera.</param>
        /// <returns>Instanca <see cref="ZahtjevFilter"/> filtera.</returns>
        public static ZahtjevFilter FromString(string s)
        {
            var filter = new ZahtjevFilter();
            if (!string.IsNullOrEmpty(s))
            {
                string[] arr = s.Split('-', StringSplitOptions.None);

                if (arr.Length == 2)
                {
                    filter.ProjektId = string.IsNullOrWhiteSpace(arr[0]) ? new int?() : int.Parse(arr[0]);
                    filter.VrstaZahtjevaId = string.IsNullOrWhiteSpace(arr[1]) ? new int?() : int.Parse(arr[1]);
                }
            }

            return filter;
        }

        /// <summary>
        /// Primjenjuje filter na upit.
        /// </summary>
        /// <param name="query">Upit za filtriranje.</param>
        /// <returns>Filtrirani upit.</returns>
        public IQueryable<ViewZahtjevInfo> Apply(IQueryable<ViewZahtjevInfo> query)
        {
            if (ProjektId.HasValue)
            {
                query = query.Where(z => z.ProjektId == ProjektId.Value);
            }
            if (VrstaZahtjevaId.HasValue)
            {
                query = query.Where(z => z.VrstaZahtjevaId == VrstaZahtjevaId.Value);
            }
            return query;
        }
    }
}

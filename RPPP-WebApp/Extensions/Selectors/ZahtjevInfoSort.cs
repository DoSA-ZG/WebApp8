using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje Zahtjeva
    /// </summary>
    public static class ZahtjevInfoSort
    {
        /// <summary>
        /// Metoda za primjenu sortiranja na upitu ViewZahtjevInfo.
        /// </summary>
        /// <param name="query">Upit koji se sortira.</param>
        /// <param name="sort">Broj koji predstavlja vrstu sortiranja.</param>
        /// <param name="ascending">True ako je sortiranje uzlazno, inače false.</param>
        /// <returns>Sortirani upit ViewZahtjevInfo.</returns>
        public static IQueryable<ViewZahtjevInfo> ApplySort(this IQueryable<ViewZahtjevInfo> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<ViewZahtjevInfo, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = z => z.ZahtjevId;
                    break;
                case 2:
                    orderSelector = z => z.Oznaka;
                    break;
                case 3:
                    orderSelector = z => z.NazivZahtjeva;
                    break;
                case 4:
                    orderSelector = z => z.NazivProjekta;
                    break;
                case 5:
                    orderSelector = z => z.NazivVrsteZahtjeva;
                    break;
                case 6:
                    orderSelector = z => z.Prioritet;
                    break;
            }
            if (orderSelector != null)
            {
                query = ascending ?
                       query.OrderBy(orderSelector) :
                       query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}

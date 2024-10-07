using RPPP_WebApp.Models;
using System;
using System.Linq;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Klasa za sortiranje Zahtjeva.
    /// </summary>
    public static class ZahtjevSort
    {
        /// <summary>
        /// Metoda za primjenu sortiranja na upitu Zahtjeva.
        /// </summary>
        /// <param name="query">Upit koji se sortira.</param>
        /// <param name="sort">Broj koji predstavlja vrstu sortiranja.</param>
        /// <param name="ascending">True ako je sortiranje uzlazno, inače false.</param>
        /// <returns>Sortirani upit Zahtjeva.</returns>
        public static IQueryable<Zahtjev> ApplySort(this IQueryable<Zahtjev> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Zahtjev, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = z => z.Oznaka;
                    break;
                case 2:
                    orderSelector = z => z.NazivZahtjeva;
                    break;
                case 3:
                    orderSelector = z => z.VrstaZahtjeva.NazivVrsteZahtjeva;
                    break;
                case 4:
                    orderSelector = z => z.Projekt.NazivProjekta;
                    break;
                case 5:
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

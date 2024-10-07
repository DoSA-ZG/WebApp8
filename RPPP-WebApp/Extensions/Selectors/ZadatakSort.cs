using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
	/// <summary>
	/// Klasa za sortiranje Zadataka
	/// </summary>
	public static class ZadatakSort
	{
        /// <summary>
        /// Metoda za primjenu sortiranja na upitu Zadatka.
        /// </summary>
        /// <param name="query">Upit koji se sortira.</param>
        /// <param name="sort">Broj koji predstavlja vrstu sortiranja.</param>
        /// <param name="ascending">True ako je sortiranje uzlazno, inače false.</param>
        /// <returns>Sortirani upit Zadatka.</returns>
        public static IQueryable<Zadatak> ApplySort(this IQueryable<Zadatak> query, int sort, bool ascending)
		{
			System.Linq.Expressions.Expression<Func<Zadatak, object>> orderSelector = null;
			switch (sort)
			{
				case 1:
					orderSelector = z => z.ZadatakId;
					break;
				//case 2:
				//	orderSelector = z => z.Opis;
				//	break;
				case 3:
					orderSelector = z => z.StatusZadatka.NazivStatusaZadatka;
					break;
				case 4:
					orderSelector = z => z.Zahtjev.Oznaka;
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

using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
	public static class ZsuradnikSort
	{
		public static IQueryable<Suradnik> ApplySort(this IQueryable<Suradnik> query, int sort, bool ascending)
		{
			Expression<Func<Suradnik, object>> orderSelector = sort switch
			{
				1 => d => d.SuradnikId,
				2 => d => d.Ime,
				3 => d => d.Prezime,
				4 => d => d.Email,
				5 => d => d.BrojMobitela,
				_ => null
			};
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

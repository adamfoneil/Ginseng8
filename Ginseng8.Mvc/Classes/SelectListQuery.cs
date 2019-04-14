using Microsoft.AspNetCore.Mvc.Rendering;
using Postulate.Base;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Classes
{
	/// <summary>
	/// Defines a query that fills a SelectList. Queries must return two columns named Value and Text
	/// </summary>
	public class SelectListQuery : Query<SelectListItem>
	{
		public SelectListQuery(string sql) : base(sql)
		{
		}

		public List<SelectListItem> ExecuteItems(IDbConnection connection)
		{
			return Execute(connection).ToList();
		}

		public async Task<List<SelectListItem>> ExecuteItemsAsync(IDbConnection connection)
		{
			var results = await ExecuteAsync(connection);
			return results.ToList();
		}

		public SelectList ExecuteSelectList(IDbConnection connection, object selectedValue = null)
		{
			IEnumerable<SelectListItem> items = Execute(connection);
			return new SelectList(items, "Value", "Text", selectedValue?.ToString());
		}

		public async Task<SelectList> ExecuteSelectListAsync(IDbConnection connection, object selectedValue = null)
		{
			IEnumerable<SelectListItem> items = await ExecuteAsync(connection);
			return new SelectList(items, "Value", "Text", selectedValue?.ToString());
		}
	}
}
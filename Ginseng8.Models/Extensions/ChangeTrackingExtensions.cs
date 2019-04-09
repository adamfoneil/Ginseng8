using Postulate.Base.Models;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Models.Extensions
{
	internal static class ChangeTrackingExtensions
	{
		/// <summary>
		/// Simpler way to check if a certain column is in the list of modified columns in an update.
		/// This would work well as a Postulate extension method, but for now it's here.
		/// </summary>
		internal static bool Include(this IEnumerable<PropertyChange> changes, string columnName)
		{
			return (changes.Any(c => c.PropertyName.Equals(columnName)));
		}

		internal static bool Include(this IEnumerable<PropertyChange> changes, string columnName, out PropertyChange change)
		{
			change = changes.FirstOrDefault(pc => pc.PropertyName.Equals(columnName));
			return (change != null);
		}

		internal static bool Include(this IEnumerable<PropertyChange> changes, IEnumerable<string> columnNames, out IEnumerable<PropertyChange> modifiedColumns)
		{
			modifiedColumns = from ch in changes
							  join col in columnNames on ch.PropertyName equals col
							  select ch;
			return modifiedColumns.Any();
		}
	}
}
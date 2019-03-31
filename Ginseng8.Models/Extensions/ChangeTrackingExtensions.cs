using Postulate.Base.Models;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Models.Extensions
{
	internal static class ChangeTrackingExtensions
	{
		internal static bool Include(this IEnumerable<PropertyChange> changes, string columnName)
		{
			return (changes.Any(c => c.PropertyName.Equals(columnName)));
		}
	}
}
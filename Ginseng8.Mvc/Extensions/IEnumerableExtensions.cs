using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Ginseng.Mvc.Extensions
{
	public static class IEnumerableExtensions
	{
		// thanks to https://www.codeproject.com/Articles/835519/Passing-Table-Valued-Parameters-with-Dapper
		public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> enumerable, string typeName, params string[] columnNames)
		{
			var dataTable = new DataTable();

			if (typeof(T).IsValueType || typeof(T).FullName.Equals("System.String"))
			{
				dataTable.Columns.Add(columnNames == null ? "NONAME" : columnNames.First(), typeof(T));
				foreach (T obj in enumerable) dataTable.Rows.Add(obj);
			}
			else
			{
				PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				PropertyInfo[] readableProperties = properties.Where(w => w.CanRead).ToArray();

				if (readableProperties.Length > 1 && columnNames == null)
				{
					throw new ArgumentException("Ordered list of column names must be provided when TVP contains more than one column");
				}

				var createColumns = (columnNames ?? readableProperties.Select(s => s.Name)).ToArray();
				foreach (string name in createColumns)
				{
					dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
				}

				foreach (T obj in enumerable)
				{
					dataTable.Rows.Add(createColumns.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj)).ToArray());
				}
			}

			return dataTable.AsTableValuedParameter(typeName);
		}
	}
}
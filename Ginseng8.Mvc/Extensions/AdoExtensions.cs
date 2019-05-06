using System;
using System.Data;
using System.Data.SqlClient;

namespace Ginseng.Mvc.Extensions
{
	/// <summary>
	/// Helper methods for getting old-school DataTables from queries
	/// handy for ClosedXML Excel downloads
	/// </summary>
	public static class AdoExtensions
	{
		public static DataTable QueryCommand(SqlCommand command)
		{
			using (var adapter = new SqlDataAdapter(command))
			{
				DataTable result = new DataTable();
				adapter.Fill(result);				
				return result;
			}
		}

		public static DataTable QueryTableWithParams(this SqlConnection connection, string selectQuery, object parameters = null)
		{
			return QueryTable(connection, selectQuery, (cmd) => SetParameters(cmd, parameters));
		}

		public static DataTable QueryTable(this SqlConnection connection, string selectQuery, Action<SqlCommand> setParameters = null)
		{
			using (var cmd = new SqlCommand(selectQuery, connection))
			{
				setParameters?.Invoke(cmd);
				return QueryCommand(cmd);
			}
		}

		private static void SetParameters(SqlCommand cmd, object parameters)
		{
			if (parameters == null) return;
			var props = parameters.GetType().GetProperties();
			foreach (var propInfo in props)
			{
				var value = propInfo.GetValue(parameters);
				cmd.Parameters.AddWithValue(propInfo.Name, (value != null) ? value : DBNull.Value);
			}
		}
	}
}
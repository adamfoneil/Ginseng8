using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace Testing
{
	[TestClass]
	public class QueryTests
	{
		// helpful sources:
		// https://www.jerriepelser.com/blog/using-configuration-files-in-dotnet-core-unit-tests/
		// https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects

		private IConfiguration _config;

		public QueryTests()
		{
			_config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
		}

		private SqlConnection GetConnection()
		{
			string connectionStr = _config.GetSection("ConnectionStrings").GetValue<string>("Default");
			return new SqlConnection(connectionStr);
		}

		[TestMethod]
		public void WorkItemsQuery()
		{
			var qry = new OpenWorkItems();
			using (var cn = GetConnection())
			{
				foreach (var testCase in OpenWorkItems.GetTestCases())
				{
					testCase.TestExecute(cn);
				}				
			}				
		}

		[TestMethod]
		public void EventLogsQuery()
		{
			var qry = new EventLogs();
			using (var cn = GetConnection())
			{
				foreach (var testCase in EventLogs.GetTestCases())
				{
					testCase.TestExecute(cn);
				}
			}
		}
	}
}

using Ginseng.Integration.Services;
using Ginseng.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.SqlServer.IntKey;
using System.Data.SqlClient;
using System.Linq;

namespace Testing
{
	[TestClass]
	public class FreshdeskTests
	{
		private readonly IConfiguration _config;

		public FreshdeskTests()
		{
			_config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
		}

		private SqlConnection GetConnection()
		{
			string connectionStr = _config.GetSection("ConnectionStrings").GetValue<string>("Default");
			return new SqlConnection(connectionStr);
		}

		[TestMethod]
		public void ListTickets()
		{
			using (var cn = GetConnection())
			{
				var org = cn.FindWhere<Organization>(new { name = "aerie" });
				var client = new FreshdeskClient(org.FreshdeskUrl, org.FreshdeskApiKey);
				var tickets = client.ListTickets();
				Assert.IsTrue(tickets.Any());
			}			
		}
	}
}
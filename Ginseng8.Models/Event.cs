using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
	public class Event : AppTable
	{
		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		public static DataTable GetSeedData()
		{
			return new Event[]
			{
				new Event() { Name = "Work Item Created" },
				new Event() { Name = "Work Item Closed" },
				new Event() { Name = "Milestone Changed" },
				new Event() { Name = "Project Changed" },
				new Event() { Name = "Comment Added" },
				new Event() { Name = "Impediment Added" },
				new Event() { Name = "Priority Changed" },
				new Event() { Name = "Hand Off" },
				new Event() { Name = "Work Item Field Changed" },
				new Event() { Name = "Milestone Date Changed" },
				new Event() { Name = "Estimate Changed" },
				new Event() { Name = "Project Added" }
			}.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
		}
	}	
}
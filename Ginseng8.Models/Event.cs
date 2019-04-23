using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
	/// <summary>
	/// I looked at using T4 to generate this, and have done so in the past,
	/// but it's way too complicated
	/// </summary>
	public enum SystemEvent
	{
		WorkItemCreated = 1,
		WorkItemClosed = 2,
		MilestoneChanged = 3,
		ProjectChanged = 4,
		CommentAdded = 5,
		ImpedimentAdded = 6,
		PriorityChanged = 7,
		HandOffForward = 8,
		WorkItemFieldChanged = 9,
		MilestoneDateChanged = 10,
		EstimateChanged = 11,
		ProjectAdded = 12,
		WorkItemOpened = 13,
		CodeCommitted = 14,
		HandOffBackward = 15,
		UserMentioned = 16
	}

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
				new Event() { Name = "Work Item Field Changed" },
				new Event() { Name = "Milestone Date Changed" },
				new Event() { Name = "Estimate Changed" },
				new Event() { Name = "Project Added" }
			}.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
		}
	}
}
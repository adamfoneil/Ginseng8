using System;

namespace Ginseng.Mvc.Classes
{
	/// <summary>
	/// Used to place work item info within the /Dashboard/Projects
	/// </summary>
	public class ProjectDashboardCell
	{
		public ProjectDashboardCell(int projectId, DateTime milestoneDate)
		{
			ProjectId = projectId;
			MilestoneDate = milestoneDate;
		}

		public int ProjectId { get; set; } // row key
		public DateTime MilestoneDate { get; set; } // column key

		public override bool Equals(object obj)
		{
			var check = obj as ProjectDashboardCell;
			return (check != null) ?
				check.ProjectId == ProjectId && check.MilestoneDate == MilestoneDate :
				false;
		}

		public override int GetHashCode()
		{
			return ProjectId.GetHashCode() + MilestoneDate.GetHashCode();
		}
	}
}
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	public class CommonDropdowns
	{
		private CommonDropdowns()
		{
		}

		public IEnumerable<Activity> AllActivities { get; set; }
		public IEnumerable<WorkItemSize> Sizes { get; set; }
		public IEnumerable<CloseReason> CloseReasons { get; set; }
		public IEnumerable<Milestone> Milestones { get; set; }
		public IEnumerable<Activity> MyActivities { get; set; }

		public static async Task<CommonDropdowns> FillAsync(SqlConnection connection, int orgId, int responsibilities)
		{
			var result = new CommonDropdowns();
			result.AllActivities = await new Activities() { OrgId = orgId, IsActive = true }.ExecuteAsync(connection);
			result.MyActivities = await new Activities() { OrgId = orgId, IsActive = true, Responsibilities = responsibilities }.ExecuteAsync(connection);
			result.Sizes = await new WorkItemSizes() { OrgId = orgId }.ExecuteAsync(connection);
			result.CloseReasons = await new CloseReasons().ExecuteAsync(connection);
			result.Milestones = await new Milestones() { OrgId = orgId }.ExecuteAsync(connection);
			return result;
		}
	}
}
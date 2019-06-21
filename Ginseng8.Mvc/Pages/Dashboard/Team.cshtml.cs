using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ginseng.Models;
using Ginseng.Mvc.Models;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	public class TeamModel : DashboardPageModel
	{
		public TeamModel(IConfiguration config) : base(config)
		{
		}

		public Dictionary<int, DefaultActivity> UserIdColumns { get; set; }

		protected override Func<ClosedWorkItemsResult, int> ClosedItemGrouping => (item) => item.DeveloperUserId ?? 0;

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			var orgUsers = await new MyOrgUsers() { OrgId = OrgId }.ExecuteAsync(connection);
			UserIdColumns = orgUsers.ToDictionary(row => row.UserId, row =>
			{
				int responsibilityId = row.Responsibilities;
				// if you have dev and biz responsibility, then assume dev
				if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;				
				return new DefaultActivity()
				{
					ActivityId = row.DefaultActivityId ?? CurrentOrg.DeveloperActivityId ?? 0,
					UserIdColumn = Responsibility.WorkItemColumnName[responsibilityId]
				};
			});
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems(QueryTraces)
			{
				OrgId = OrgId,
                TeamId = CurrentOrgUser.CurrentTeamId,
				AppId = CurrentOrgUser.EffectiveAppId,
				LabelId = LabelId,
				HasAssignedUserId = true
			};
		}
	}
}
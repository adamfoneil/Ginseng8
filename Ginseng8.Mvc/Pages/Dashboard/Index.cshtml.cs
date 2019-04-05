using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
	[Authorize]
	public class MyItemsModel : DashboardPageModel
	{
		public MyItemsModel(IConfiguration config) : base(config)
		{
		}

		/// <summary>
		/// What column is user Id assigned to when items are created?
		/// </summary>
		public string UserIdColumnName { get; set; }

		/// <summary>
		/// All my hand-off activities
		/// </summary>
		public IEnumerable<ActivitySubscription> MyActivitySubscriptions { get; set; }
		
		public IEnumerable<OpenWorkItemsResult> PausedItems { get; set; }

		public HtmlString MyHandOffActivityList()
		{
			string result = (MyActivitySubscriptions?.Any() ?? false) ?
				string.Join(", ", MyActivitySubscriptions
					.GroupBy(asub => asub.ActivityId)
					.Select(grp => $"<strong>{grp.First().ActivityName}:</strong> {string.Join(", ", grp.Select(asub => asub.AppName))}")) :
				"none";

			return new HtmlString(result);
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				AssignedUserId = UserId,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};
		}

		protected override void OnGetInternal(SqlConnection connection)
		{
			int responsibilityId = CurrentOrgUser.Responsibilities;
			// if you have dev and biz responsibility, then assume dev
			if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;
			UserIdColumnName = Responsibility.WorkItemColumnName[responsibilityId];
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			MyActivitySubscriptions = await new MyHandOffActivities() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(connection);

			PausedItems = await new OpenWorkItems() { OrgId = OrgId, IsPaused = true }.ExecuteAsync(connection);
		}
	}
}
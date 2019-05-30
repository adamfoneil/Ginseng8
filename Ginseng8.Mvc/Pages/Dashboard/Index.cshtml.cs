using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
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
		
		/// <summary>
		/// Items in activities that I follow that are paused
		/// </summary>
		public IEnumerable<OpenWorkItemsResult> MyHandOffItems { get; set; }
		public ILookup<int, Label> HandOffLabels { get; set; }
		public ILookup<int, Comment> HandOffComments { get; set; }        
        public IEnumerable<MyWorkScheduleResult> MySchedule { get; set; }
        public int UnestimatedItemCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool NeedsEstimate { get; set; }

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
			var result = new OpenWorkItems(QueryTraces)
			{
				OrgId = OrgId,
				AssignedUserId = UserId,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};

            if (Date.HasValue)
            {
                result.WithWorkSchedule = true;
                result.ScheduleUserId = UserId;
                result.WorkDate = Date;
            }

            if (NeedsEstimate) result.SizeId = 0;

            return result;
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
            UnestimatedItemCount = WorkItems.Where(wi => wi.EstimateHours == 0).Count();

			MyActivitySubscriptions = await new MyHandOffActivities() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(connection);
			MyHandOffItems = await new OpenWorkItems(QueryTraces) { OrgId = OrgId, InMyActivities = true, ActivityUserId = UserId, IsPaused = true, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);

			var itemIds = MyHandOffItems.Select(wi => wi.Id).ToArray();
			var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = OrgId }.ExecuteAsync(connection);
			HandOffLabels = labelsInUse.ToLookup(row => row.WorkItemId);

			var comments = await new Comments() { OrgId = OrgId, ObjectIds = itemIds, ObjectType = ObjectType.WorkItem }.ExecuteAsync(connection);
			HandOffComments = comments.ToLookup(row => row.ObjectId);

            MySchedule = await new MyWorkSchedule() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(connection);
		}
	}
}
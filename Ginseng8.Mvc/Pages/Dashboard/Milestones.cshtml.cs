using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
	[Authorize]
	public class MilestonesModel : DashboardPageModel
	{
		public MilestonesModel(IConfiguration config) : base(config)
		{
            ShowExcelDownload = Id.HasValue;
		}

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool? PastDue { get; set; }

        public IEnumerable<Milestone> Milestones { get; private set; }
        public Dictionary<int, MilestoneMetricsResult> Metrics { get; private set; }

        public Milestone NextSoonest { get; private set; }
		public Milestone NextGenerated { get; private set; }

		public IEnumerable<OpenWorkItemsResult> EmptyMilestones { get; set; }
		public IEnumerable<OpenWorkItemsResult> BacklogItems { get; set; }

		protected override Func<ClosedWorkItemsResult, int> ClosedItemGrouping => (row) => row.MilestoneId ?? 0;

		protected override OpenWorkItems GetQuery()
		{
            if (Id.HasValue)
            {
                return new OpenWorkItems(QueryTraces)
                {
                    OrgId = OrgId,
                    MilestoneId = Id.Value,                    
                    LabelId = LabelId,
                    IsPastDue = PastDue
                };
            }

            return null;
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
            Milestones = await new Milestones() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);

            var metrics = (!Id.HasValue) ?
                await new MilestoneMetrics() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection) :
                await new MilestoneMetrics() { OrgId = OrgId, Id = Id }.ExecuteAsync(connection);

            Metrics = metrics.ToDictionary(row => row.Id);

            var emptyMilestones = await new Milestones() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId ?? 0, HasWorkItems = false }.ExecuteAsync(connection);
			EmptyMilestones = emptyMilestones.Select(ms => new OpenWorkItemsResult()
			{
				MilestoneId = ms.Id,
				MilestoneDate = ms.Date,
				MilestoneDaysAway = ms.DaysAway,
				MilestoneName = ms.Name
			});

			NextSoonest = await Milestone.GetSoonestNextAsync(connection, OrgId);
			NextGenerated = await Milestone.CreateNextAsync(connection, OrgId);
		}

		public async Task<IActionResult> OnPostCreate(Milestone record, string returnUrl)
		{
            record.ApplicationId = CurrentOrgUser.CurrentAppId ?? 0;
			await Data.TrySaveAsync(record);
			return Redirect(returnUrl);
		}

		public async Task<IActionResult> OnPostMoveToNextMilestone(int appId, int fromMilestoneId, int toMilestoneId)
		{
			return await UpdateMilestoneInner(appId, fromMilestoneId, toMilestoneId);
		}

		public async Task<IActionResult> OnPostMoveToNewMilestone(int appId, int fromMilestoneId, string toName, DateTime toDate)
		{
			var newMs = new Milestone()
			{
				ApplicationId = OrgId,
				Name = toName,
				Date = toDate
			};

			await Data.TrySaveAsync(newMs);

			return await UpdateMilestoneInner(appId, fromMilestoneId, newMs.Id);
		}

		private async Task<IActionResult> UpdateMilestoneInner(int appId, int fromMilestoneId, int toMilestoneId)
		{
			using (var cn = Data.GetConnection())
			{
				var workItems = await new OpenWorkItems() { OrgId = OrgId, AppId = appId, MilestoneId = fromMilestoneId }.ExecuteAsync(cn);
				foreach (var openItem in workItems)
				{
					// note that query results are read-only, so we need a separate query to the updateable work item
					var item = await cn.FindAsync<Ginseng.Models.WorkItem>(openItem.Id);
					item.MilestoneId = toMilestoneId;
					// this assures that event log happens
					await Data.TryUpdateAsync(item, r => r.MilestoneId);
				}

				return RedirectToPage("Milestones");
			}
		}
	}
}
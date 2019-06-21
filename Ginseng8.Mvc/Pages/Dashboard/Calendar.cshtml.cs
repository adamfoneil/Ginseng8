using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
	public class CalendarModel : DashboardPageModel
	{
		public CalendarModel(IConfiguration config) : base(config)
		{
            ShowExcelDownload = Id.HasValue;
		}

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool? PastDue { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? ProjectId { get; set; }

        public IEnumerable<Project> FilterProjects { get; set; } // projects with milestones
        public Milestone SelectedMilestone { get; private set; }
        public IEnumerable<Milestone> Milestones { get; private set; }
        public Dictionary<int, MilestoneMetricsResult> Metrics { get; private set; }
        public ILookup<int, string> ProjectNicknames { get; set; }
        public ILookup<int, DevMilestoneWorkingHoursResult> DevLoad { get; set; }

        public SelectList ProjectSelect { get; set; }

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
                    IsPastDue = PastDue,
                    TeamId = CurrentOrgUser.CurrentTeamId,
                    AppId = CurrentOrgUser.CurrentAppId
                };
            }

            return null;
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
            Milestones = await new Milestones() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId, ProjectId = ProjectId }.ExecuteAsync(connection);

            if (Id.HasValue)
            {
                SelectedMilestone = await connection.FindAsync<Milestone>(Id.Value);
            }

            // for the create milestone partial
            ProjectSelect = await new ProjectSelect() { TeamId = CurrentOrgUser.CurrentTeamId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteSelectListAsync(connection);

            if (!Id.HasValue && CurrentOrgUser.CurrentAppId.HasValue)
            {
                FilterProjects = await new Projects() { AppId = CurrentOrgUser.CurrentAppId, HasMilestones = true, IsActive = true }.ExecuteAsync(connection);
            }            

            var metrics = (!Id.HasValue) ?
                await new MilestoneMetrics() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection) :
                await new MilestoneMetrics() { OrgId = OrgId, Id = Id }.ExecuteAsync(connection);

            Metrics = metrics.ToDictionary(row => row.Id);

            var emptyMilestones = await new Milestones() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId ?? 0, HasOpenWorkItems = false, Id = Id }.ExecuteAsync(connection);
			EmptyMilestones = emptyMilestones.Select(ms => new OpenWorkItemsResult()
			{
				MilestoneId = ms.Id,
				MilestoneDate = ms.Date,
				MilestoneDaysAway = ms.DaysAway,
				MilestoneName = ms.Name
			});

            var load = await new DevMilestoneWorkingHours() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
            DevLoad = load.ToLookup(row => row.MilestoneId);

			NextSoonest = await Milestone.GetSoonestNextAsync(connection, OrgId);
			NextGenerated = await Milestone.CreateNextAsync(connection, OrgId);
		}

		public async Task<IActionResult> OnPostCreate(Milestone record, string returnUrl)
		{
            record.ApplicationId = CurrentOrgUser.CurrentAppId ?? 0;
            using (var cn = Data.GetConnection())
            {
                if (await Data.TrySaveAsync(cn, record))
                {
                    if (record.ProjectId != 0) await CreatePlaceholderItemAsync(cn, record);
                }
            }
			
			return Redirect(returnUrl);
		}

        private async Task CreatePlaceholderItemAsync(SqlConnection cn, Milestone record)
        {
            const string text = @"Placeholder item created with milestone. This enables you to filter for this project on the milestone dashboard. This will automatically close when you add another work item to this milestone.";

            var app = await cn.FindAsync<Application>(record.ApplicationId);

            var workItem = new Ginseng.Models.WorkItem()
            {
                OrganizationId = OrgId,                
                TeamId = app.TeamId ?? 0,
                ApplicationId = record.ApplicationId,
                MilestoneId = record.Id,
                ProjectId = record.ProjectId,
                Title = "Placeholder item created with milestone",
                HtmlBody = $"<p>{text}</p>",
                TextBody = text
            };

            await workItem.SetNumberAsync(cn);

            if (await Data.TrySaveAsync(cn, workItem))
            {
                var wil = new WorkItemLabel()
                {
                    WorkItemId = workItem.Id,
                    LabelId = await GetPlaceholderLabelIdAsync(cn)
                };

                await Data.TrySaveAsync(cn, wil);
            }
        }

        private async Task<int> GetPlaceholderLabelIdAsync(SqlConnection cn)
        {
            var label = 
                await cn.FindWhereAsync<Label>(new { OrganizationId = OrgId, Name = Label.PlaceholderLabel }) ?? 
                new Label() { OrganizationId = OrgId, Name = Label.PlaceholderLabel, BackColor = "#B8B8B8", ForeColor = "black", IsActive = false };

            if (label.Id == 0) await cn.SaveAsync(label, CurrentUser);

            return label.Id;
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

        /// <summary>
        /// returns the row number for a given yearMonth
        /// </summary>
        public int RowNumber(YearMonth yearMonth, int countPerRow = 4)
        {
            int result = yearMonth.Index / countPerRow;
            return result;
        }

        public IEnumerable<YearMonth> GetYearMonthRange(int futureMonths = 0)
        {
            if (Milestones?.Any() ?? false)
            {
                var start = Milestones.First().Date;
                var end = Milestones.Last().Date.AddMonths(futureMonths);
                return GetYearMonthRange(new YearMonth(start), new YearMonth(end));
            }

            return Enumerable.Empty<YearMonth>();
        }

        private IEnumerable<YearMonth> GetYearMonthRange(YearMonth start, YearMonth end)
        {
            List<YearMonth> results = new List<YearMonth>();
            YearMonth add = start;                        
            while (add <= end)
            {
                results.Add(add);
                add += 1;                                               
            }
            return results;
        }

        public ILookup<YearMonth, Milestone> GetCalendar()
        {
            return Milestones.ToLookup(row => new YearMonth(row.Date));
        }
    }
}
using ClosedXML.Extensions;
using Ginseng.Models;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postulate.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
    [Authorize]
    public abstract class DashboardPageModel : AppPageModel
    {
        public DashboardPageModel(IConfiguration config) : base(config)
        {
        }

        public bool ShowLabelFilter { get; set; } = true;
        public bool ShowExcelDownload { get; set; } = true;
        public IEnumerable<OpenWorkItemsResult> WorkItems { get; set; }
        public ILookup<int, Label> SelectedLabels { get; set; }
        public IEnumerable<IGrouping<int, Label>> LabelFilter { get; set; }
        public CommonDropdowns Dropdowns { get; set; }
        public ILookup<int, Comment> Comments { get; set; }
        public ILookup<int, ClosedWorkItemsResult> ClosedItems { get; set; }
        public IEnumerable<WorkDaysResult> WorkDays { get; set; }
        public Dictionary<int, MilestoneMetricsResult> MilestoneMetrics { get; set; }        

        /// <summary>
        /// triggers display of partial to offer to move items to soonest upcoming or new milestone
        /// </summary>
        public IEnumerable<OpenWorkItemsResult> ItemsInPastMilestone { get; private set; }

        public Milestone NextSoonestMilestone { get; private set; }

        [BindProperty(SupportsGet = true)]
        public int? LabelId { get; set; }

        /// <summary>
        /// Implement this to get the query for the dashboard
        /// </summary>
        protected abstract OpenWorkItems GetQuery();

        protected virtual Func<ClosedWorkItemsResult, int> ClosedItemGrouping
        {
            get { return null; }
        }

        /// <summary>
        /// Override this to populate additional model properties during the OnGetAsync method
        /// </summary>
        protected virtual async Task OnGetInternalAsync(SqlConnection connection)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Override this to populate individual model properties that won't benefit from async execution
        /// </summary>
        protected virtual void OnGetInternal(SqlConnection connection)
        {
            // do nothing by default
        }

        /// <summary>
        /// Override this if you need to redirect from the dashboard page for some reason (e.g. if a search was done whose results can't display on this page)
        /// </summary>
        protected virtual async Task<RedirectResult> GetRedirectAsync(SqlConnection connection)
        {
            return await Task.FromResult<RedirectResult>(null);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                var redirect = await GetRedirectAsync(cn);
                if (redirect != null) return redirect;

                var query = GetQuery();
                if (query != null)
                {
                    WorkItems = await GetQuery().ExecuteAsync(cn);

                    ItemsInPastMilestone = WorkItems.Where(wi => wi.MilestoneDate < DateTime.Today).ToArray();
                    if (ItemsInPastMilestone.Any())
                    {
                        var appId = CurrentOrgUser.CurrentAppId ?? 0;
                        if (appId != 0)
                        {
                            NextSoonestMilestone = await Milestone.GetSoonestNextAsync(cn, appId) ?? await Milestone.CreateNextAsync(cn, appId);
                        }                        
                    }

                    int[] itemIds = WorkItems.Select(wi => wi.Id).ToArray();
                    var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = OrgId }.ExecuteAsync(cn);
                    SelectedLabels = labelsInUse.ToLookup(row => row.WorkItemId);
                    LabelFilter = labelsInUse.GroupBy(row => row.Id);

                    var comments = await new Comments() { OrgId = OrgId, ObjectIds = itemIds, ObjectType = ObjectType.WorkItem }.ExecuteAsync(cn);
                    Comments = comments.ToLookup(row => row.ObjectId);

                    WorkDays = await new WorkDays() { OrgId = OrgId }.ExecuteAsync(cn);

                    if (ClosedItemGrouping != null)
                    {
                        var closedItems = await new ClosedWorkItems() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(cn);
                        ClosedItems = closedItems.ToLookup(row => ClosedItemGrouping(row));
                    }

                    var milestoneIds = WorkItems.GroupBy(row => row.MilestoneId).Select(grp => grp.Key).ToArray();
                    var milestoneMetrics = await new MilestoneMetrics() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId, MilestoneIds = milestoneIds }.ExecuteAsync(cn);
                    MilestoneMetrics = milestoneMetrics.ToDictionary(row => row.Id);
                }

                Dropdowns = await CommonDropdowns.FillAsync(cn, OrgId);

                await OnGetInternalAsync(cn);

                OnGetInternal(cn);
            }

            return Page();
        }

        public LoadView GetLoadView(IGrouping<int, OpenWorkItemsResult> milestoneGrp, MilestoneMetricsResult metrics = null)
        {
            int estimateHours = milestoneGrp.Sum(wi => wi.EstimateHours);

            LoadView result = new LoadView()
            {
                EstimateHours = estimateHours
            }; 

            if (metrics != null)
            {
                result.MilestoneMetrics = metrics;
            }
            else if (MilestoneMetrics.ContainsKey(milestoneGrp.Key))
            {
                result.MilestoneMetrics = MilestoneMetrics[milestoneGrp.Key];
            }

            return result;
        }

        public FileResult OnGetExcelDownload()
        {
            // get the query this dashboard uses
            var qry = GetQuery();

            // get the query's underlying SQL, resolving conditional criteria and parameters
            var sql = QueryUtil.ResolveSql(qry.Sql, qry);

            using (var cn = Data.GetConnection())
            {
                // query the data and return a data table
                var data = cn.QueryTableWithParams(sql, qry);
                data.TableName = "Open Work Items";

                var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.AddWorksheet(data);
                return wb.Deliver("OpenWorkItems.xlsx");
            }
        }
    }
}
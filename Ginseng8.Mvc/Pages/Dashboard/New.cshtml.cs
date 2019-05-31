using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class NewModel : DashboardPageModel
    {
        public NewModel(IConfiguration config) : base(config)
        {
            ShowLabelFilter = false;
        }

        public IEnumerable<Label> Labels { get; set; }
        public IEnumerable<Application> Applications { get; set; }        
        public ILookup<AppLabelCell, OpenWorkItemsResult> AppLabelItems { get; set; }
        public Dictionary<int, LabelInstructions> LabelInstructions { get; set; }

        public IEnumerable<OpenWorkItemsResult> GetAppLabelItems(int applicationId, int labelId)
        {
            var cell = new AppLabelCell(applicationId, labelId);
            return AppLabelItems[cell];
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            Applications = await new Applications() { OrgId = OrgId, IsActive = true, AllowNewItems = true }.ExecuteAsync(connection);

            var workItemLabelMap = SelectedLabels
                .Select(grp => new { WorkItemId = grp.Key, LabelId = grp.First().Id })
                .ToDictionary(row => row.WorkItemId, row => row.LabelId);

            AppLabelItems = WorkItems.ToLookup(row => new AppLabelCell(row.ApplicationId, workItemLabelMap[row.Id]));

            var instructions = await new MyLabelInstructions() { OrgId = OrgId }.ExecuteAsync(connection);
            LabelInstructions = instructions.ToDictionary(row => row.LabelId);
        }

        protected override OpenWorkItems GetQuery()
        {
            int[] labelIds = null;
            using (var cn = Data.GetConnection())
            {
                Labels = new Labels() { OrgId = OrgId, IsActive = true, AllowNewItems = true }.Execute(cn);
                labelIds = Labels.Select(l => l.Id).ToArray();
            }

            return new OpenWorkItems()
            {
                OrgId = OrgId,
                HasFutureMilestone = false,
                DataEntryApps = true,
                LabelIds = labelIds
            };
        }
    }
}
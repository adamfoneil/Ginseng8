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
            ShowExcelDownload = false;
        }
               
        public int TeamId { get; set; }
        public IEnumerable<Application> Applications { get; set; }
        public ILookup<int, Label> Labels { get; set; }
        public ILookup<TeamLabelCell, OpenWorkItemsResult> TeamLabelItems { get; set; }
        public Dictionary<int, LabelInstructions> LabelInstructions { get; set; }

        public IEnumerable<OpenWorkItemsResult> GetTeamLabelItems(int teamId, int labelId)
        {
            var cell = new TeamLabelCell(teamId, labelId);
            return TeamLabelItems[cell];
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            TeamId = CurrentOrgUser.CurrentTeamId ?? 0;
            Applications = await new Applications() { OrgId = OrgId, AllowNewItems = true, TeamId = TeamId, IsActive = true }.ExecuteAsync(connection);

            var workItemLabelMap = SelectedLabels
                .Select(grp => new { WorkItemId = grp.Key, LabelId = grp.First().Id })
                .ToDictionary(row => row.WorkItemId, row => row.LabelId);

            TeamLabelItems = WorkItems.ToLookup(row => new TeamLabelCell(row.TeamId, workItemLabelMap[row.Id]));

            var instructions = await new MyLabelInstructions() { OrgId = OrgId }.ExecuteAsync(connection);
            LabelInstructions = instructions.ToDictionary(row => row.LabelId);
        }

        protected override OpenWorkItems GetQuery()
        {
            int[] labelIds = null;
            using (var cn = Data.GetConnection())
            {
                var labels = new NewItemAppLabels() { OrgId = OrgId }.Execute(cn);
                Labels = labels.ToLookup(row => row.ApplicationId);
                labelIds = labels.GroupBy(row => row.Id).Select(grp => grp.Key).ToArray();
            }

            return new OpenWorkItems()
            {
                OrgId = OrgId,
                HasProject = false,
                DataEntryApps = true,
                LabelIds = labelIds,
                HasAssignedUserId = false,
                HasPriority = false
            };
        }
    }
}
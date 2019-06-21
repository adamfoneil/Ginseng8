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
        public IEnumerable<Team> Teams { get; set; }
        public IEnumerable<Application> Applications { get; set; }
        public ILookup<int, Label> Labels { get; set; }
        public ILookup<AppLabelCell, OpenWorkItemsResult> AppLabelItems { get; set; }
        public Dictionary<int, LabelInstructions> LabelInstructions { get; set; }
        public Dictionary<int, string> LabelNotifyUsers { get; set; }

        public IEnumerable<OpenWorkItemsResult> GetAppLabelItems(int appId, int labelId)
        {
            var cell = new AppLabelCell(appId, labelId);
            return AppLabelItems[cell];
        }

        public string GetLabelNotifyUsers(int labelId)
        {
            return LabelNotifyUsers.ContainsKey(labelId) ? LabelNotifyUsers[labelId] : "no one currently setup";
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            TeamId = CurrentOrgUser.CurrentTeamId ?? 0;
            Applications = await new Applications() { OrgId = OrgId, AllowNewItems = true, TeamId = TeamId, IsActive = true }.ExecuteAsync(connection);
            Teams = await new Teams() { OrgId = OrgId, IsActive = true }.ExecuteAsync(connection);

            if (!Applications.Any() && TeamId != 0)
            {
                Applications = new Application[]
                {
                    new Application()
                    {
                        Name = $"Enter New {CurrentOrgUser.CurrentTeam.Name} work item"
                    }
                };
            }

            var workItemLabelMap = SelectedLabels
                .Select(grp => new { WorkItemId = grp.Key, LabelId = grp.First().Id })
                .ToDictionary(row => row.WorkItemId, row => row.LabelId);

            var notifyResults = await new LabelSubscriptionUsers() { OrgId = OrgId }.ExecuteAsync(connection);
            var notifyUserLookup = notifyResults.ToLookup(row => row.LabelId);
            LabelNotifyUsers = notifyUserLookup.ToDictionary(row => row.Key, items => string.Join(", ", items.Select(u => u.UserName)));

            AppLabelItems = WorkItems.ToLookup(row => new AppLabelCell(row.ApplicationId, workItemLabelMap[row.Id]));

            var instructions = await new MyLabelInstructions() { OrgId = OrgId }.ExecuteAsync(connection);
            LabelInstructions = instructions.ToDictionary(row => row.LabelId);
        }

        protected override OpenWorkItems GetQuery()
        {
            int[] labelIds = null;
            using (var cn = Data.GetConnection())
            {
                var appLabels = new NewItemAppLabels() { OrgId = OrgId }.Execute(cn);
                var teamLabels = new TeamLabels() { OrgId = OrgId, TeamId = CurrentOrgUser.CurrentTeamId ?? 0 }.Execute(cn);
                var allLabels = appLabels.Concat(teamLabels);
                Labels = allLabels.ToLookup(row => row.ApplicationId);
                labelIds = allLabels.GroupBy(row => row.Id).Select(grp => grp.Key).ToArray();
            }

            return new OpenWorkItems(QueryTraces)
            {
                OrgId = OrgId,
                TeamId = CurrentOrgUser.CurrentTeamId,
                HasProject = false,
                LabelIds = labelIds,
                HasAssignedUserId = false,
                HasPriority = false
            };
        }
    }
}
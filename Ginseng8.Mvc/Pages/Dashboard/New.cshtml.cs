using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
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
        public int AppId { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        public Dictionary<int, Team> TeamInfo { get; set; }
        public IEnumerable<Application> Applications { get; set; }
        public Application SelectedApp { get; set; }
        public ILookup<int, Label> Labels { get; set; } // by appId        
        public ILookup<AppLabelCell, OpenWorkItemsResult> AppLabelItems { get; set; }
        public Dictionary<int, LabelInstructions> LabelInstructions { get; set; }
        public Dictionary<int, string> LabelNotifyUsers { get; set; }

        private IEnumerable<Label> NewItemLabels { get; set; }

        public IEnumerable<OpenWorkItemsResult> GetAppLabelItems(int appId, int labelId)
        {
            var cell = new AppLabelCell(appId, labelId);
            return AppLabelItems[cell];
        }

        public string GetLabelNotifyUsers(int labelId)
        {
            return LabelNotifyUsers.ContainsKey(labelId) ? LabelNotifyUsers[labelId] : "no one currently setup";
        }

        public bool UseApplications(int? teamId)
        {
            return (teamId.HasValue) ? TeamInfo[teamId.Value].UseApplications : true;
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            TeamId = CurrentOrgUser.CurrentTeamId ?? 0;            
            Applications = await new Applications() { OrgId = OrgId, AllowNewItems = true, TeamId = TeamId, IsActive = true }.ExecuteAsync(connection);
            Teams = await new Teams() { OrgId = OrgId, IsActive = true }.ExecuteAsync(connection);
            TeamInfo = Teams.ToDictionary(row => row.Id); // used to give access to Team.UseApplications for determining whether to show app dropdown in new item form

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

            SelectedApp = await Data.FindAsync<Application>(AppId);

            var workItemLabelMap = SelectedLabels
                .Select(grp => new { WorkItemId = grp.Key, LabelId = GetFilteredLabelId(grp) })
                .ToDictionary(row => row.WorkItemId, row => row.LabelId);

            var notifyResults = await new LabelSubscriptionUsers() { OrgId = OrgId }.ExecuteAsync(connection);
            var notifyUserLookup = notifyResults.ToLookup(row => row.LabelId);
            LabelNotifyUsers = notifyUserLookup.ToDictionary(row => row.Key, items => string.Join(", ", items.Select(u => u.UserName)));

            AppLabelItems = WorkItems.ToLookup(row => new AppLabelCell(row.ApplicationId, workItemLabelMap[row.Id]));

            var instructions = await new MyLabelInstructions() { OrgId = OrgId }.ExecuteAsync(connection);
            LabelInstructions = instructions.ToDictionary(row => row.LabelId);
        }

        /// <summary>
        /// A work may have any number of labels in any order.
        /// For display purposes on this page, we need to look for labels that this page is specifically filtering for.
        /// We used to just get the "first" label, but this might not be a label we're displaying on New Items,
        /// causing the item to not show as expected.
        /// </summary>
        private int GetFilteredLabelId(IGrouping<int, Label> grp)
        {
            // idea here, is to look for one of the labels we're displaying, then fall back to the "first" 
            return (grp.FirstOrDefault(lbl => NewItemLabels.Contains(lbl)) ?? grp.First()).Id;
        }

        protected override OpenWorkItems GetQuery()
        {
            int[] labelIds = null;
            using (var cn = Data.GetConnection())
            {
                var appLabels = new NewItemAppLabels() { OrgId = OrgId }.Execute(cn);
                var teamLabels = new TeamLabels() { OrgId = OrgId, TeamId = CurrentOrgUser.CurrentTeamId ?? 0 }.Execute(cn);
                NewItemLabels = appLabels.Concat(teamLabels);
                Labels = NewItemLabels.ToLookup(row => row.ApplicationId);
                labelIds = NewItemLabels.GroupBy(row => row.Id).Select(grp => grp.Key).ToArray();
            }

            TeamId = CurrentOrgUser.CurrentTeamId ?? 0;
            AppId = CurrentOrgUser.CurrentAppId ?? 0;

            return new OpenWorkItems(QueryTraces)
            {
                OrgId = OrgId,
                TeamId = TeamId,
                AppId = AppId,
                HasProject = false,
                LabelIds = labelIds,
                HasAssignedUserId = false
            };
        }
    }
}
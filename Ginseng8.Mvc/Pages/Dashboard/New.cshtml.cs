using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
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
    public class NewModel : DashboardPageModel
    {
        public NewModel(IConfiguration config) : base(config)
        {
            ShowLabelFilter = false;
            ShowExcelDownload = false;
        }
        
        public int TeamId { get; set; }
        public int AppId { get; set; }

        [BindProperty(SupportsGet = true, Name = "Id")]
        public int FilterLabelId { get; set; }

        public Label SelectedLabel { get; set; }

        public IEnumerable<Team> Teams { get; set; }
        public Dictionary<int, Team> TeamInfo { get; set; }
        public IEnumerable<Application> Applications { get; set; }
        public Application SelectedApp { get; set; }
        public ILookup<int, Label> Labels { get; set; } // by appId        
        public ILookup<AppLabelCell, OpenWorkItemsResult> AppLabelItems { get; set; }
        public Dictionary<int, LabelInstructions> LabelInstructions { get; set; }
        public Dictionary<int, string> LabelNotifyUsers { get; set; }
        public Dictionary<int, int> OpenItemCounts { get; set; }

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

        public int GetOpenItemCount(int labelId)
        {
            return (OpenItemCounts.ContainsKey(labelId)) ? OpenItemCounts[labelId] : 0;
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            TeamId = CurrentOrgUser.CurrentTeamId ?? 0;

            SelectedLabel = await connection.FindAsync<Label>(FilterLabelId);

            var appLabels = await new NewItemAppLabels() { OrgId = OrgId }.ExecuteAsync(connection);
            var teamLabels = await new TeamLabels() { OrgId = OrgId, TeamId = TeamId }.ExecuteAsync(connection);
            NewItemLabels = appLabels.Concat(teamLabels);
            Labels = NewItemLabels.ToLookup(row => row.ApplicationId);

            var labelCounts = await new ItemCountsByLabel()
            {
                OrgId = OrgId,
                TeamId = CurrentOrgUser.CurrentTeamId ?? 0,
                AppId = CurrentOrgUser.EffectiveAppId,
                HasProject = false,
                HasAssignedUserId = false
            }.ExecuteAsync(connection);
            OpenItemCounts = labelCounts.ToDictionary(row => row.LabelId, row => row.Count);

            Applications = await new Applications() { OrgId = OrgId, AllowNewItems = true, TeamId = TeamId, IsActive = true }.ExecuteAsync(connection);
            Teams = await new Teams() { OrgId = OrgId, IsActive = true }.ExecuteAsync(connection);
            TeamInfo = Teams.ToDictionary(row => row.Id); // used to give access to Team.UseApplications for determining whether to show app dropdown in new item form

            if (!Applications.Any() && TeamId != 0)
            {
                SelectedApp = new Application()
                {
                    Name = $"Enter New {CurrentOrgUser.CurrentTeam.Name} work item"
                };                
            }

            if (AppId != 0 && SelectedApp == null)
            {
                SelectedApp = await Data.FindAsync<Application>(AppId);
            }
            
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
            TeamId = CurrentOrgUser.CurrentTeamId ?? 0;
            AppId = CurrentOrgUser.EffectiveAppId ?? 0;

            var qry = new OpenWorkItems(QueryTraces)
            {
                OrgId = OrgId,
                TeamId = TeamId,      
                AppId = CurrentOrgUser.EffectiveAppId,
                HasProject = false,
                LabelId = FilterLabelId,
                HasAssignedUserId = false
            };            

            return qry;
        }
    }
}
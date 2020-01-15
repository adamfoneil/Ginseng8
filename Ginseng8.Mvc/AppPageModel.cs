using Ginseng.Models;
using Ginseng.Mvc.Attributes;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postulate.Base.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.QX.Models;

namespace Ginseng.Mvc
{
    public class AppPageModel : PageModel, IUserInfo
    {
        public AppPageModel(IConfiguration config)
        {
            Data = new DataAccess(config);
        }

        public string OrgName => Data.CurrentOrg?.Name ?? "Ginseng";

        protected DataAccess Data { get; }
        public Organization CurrentOrg { get { return Data.CurrentOrg; } }
        public UserProfile CurrentUser { get { return Data.CurrentUser; } }
        public OrganizationUser CurrentOrgUser { get { return Data.CurrentOrgUser; } }
        public int UserId { get { return CurrentUser?.UserId ?? 0; } }
        public int OrgId { get { return CurrentUser?.OrganizationId ?? 0; } }
        public DateTime LocalTime { get { return CurrentUser.LocalTime; } }
        public Dictionary<string, UserOptionValue> Options { get; set; }
        public bool HasNotifications { get; private set; }

        public string TeamUseApps { get; private set; } // this enables JS show-hides of app dropdown based on team selection

        public IEnumerable<SelectListItem> AssignToUsers { get; set; }

        /// <summary>
        /// For populating home page button with org-switch links
        /// </summary>
        public IEnumerable<Organization> SwitchOrgs { get; set; }

        public List<QueryTrace> QueryTraces { get; private set; } = new List<QueryTrace>();

        public async Task<SelectList> CurrentOrgAppSelectAsync()
        {
            using (var cn = Data.GetConnection())
            {
                return await new AppSelect() { OrgId = OrgId, TeamId = CurrentOrgUser.CurrentTeamId }.ExecuteSelectListAsync(cn, CurrentOrgUser?.CurrentAppId);
            }
        }

        public async Task<SelectList> CurrentOrgTeamSelectAsync()
        {
            using (var cn = Data.GetConnection())
            {
                return await new TeamSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, CurrentOrgUser?.CurrentTeamId);
            }
        }

        public async Task<WorkItem> FindWorkItemAsync(int number)
        {
            return await Data.FindWhereAsync<WorkItem>(new { OrganizationId = OrgId, Number = number });
        }

        public async Task<OpenWorkItemsResult> FindWorkItemResultAsync(int number)
        {
            using (var cn = Data.GetConnection())
            {
                return await new OpenWorkItems() { OrgId = OrgId, Number = number }.ExecuteSingleAsync(cn);
            }
        }

        protected bool IsMyResponsibility(int responsibilityId)
        {
            throw new NotImplementedException();
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);

            if (User.Identity.IsAuthenticated)
            {
                Data.Initialize(User, TempData);

                if (IsOrgRequired(context) && !CurrentUser.OrganizationId.HasValue)
                {
                    context.Result = new RedirectResult("/Setup/Organization?mustCreate=true");
                    return;
                }

                using (var cn = Data.GetConnection())
                {
                    SwitchOrgs = new MySwitchOrgs() { CurrentOrgId = OrgId, UserId = UserId }.Execute(cn);
                    AssignToUsers = new UserSelect() { OrgId = OrgId, IsEnabled = true }.ExecuteItems(cn);

                    var teams = new Teams() { OrgId = OrgId }.Execute(cn);
                    TeamUseApps = JsonConvert.SerializeObject(teams.ToDictionary(row => row.Id, row => row.UseApplications));

                    var options = new MyOptions() { UserId = UserId }.Execute(cn);
                    Options = options.ToDictionary(row => row.OptionName);

                    HasNotifications = cn.RowExists("[dbo].[Notification] WHERE [SendTo]=@userName AND [Method]=3 AND [DateDelivered] IS NULL", new { userName = User.Identity.Name });
                }
            }
        }

        private bool IsOrgRequired(PageHandlerExecutingContext context)
        {
            var attrs = context.ActionDescriptor.HandlerMethods.SelectMany(m => m.MethodInfo.GetCustomAttributes(typeof(OrgNotRequired), true));
            if (attrs.Any()) return false;

            attrs = context.HandlerInstance.GetType().GetCustomAttributes(typeof(OrgNotRequired), false);
            if (attrs.Any()) return false;

            return true;
        }

        public async Task<string> GetUserDisplayName(int orgId, string userName)
        {
            using (var cn = Data.GetConnection())
            {
                return await OrganizationUser.GetUserDisplayNameAsync(cn, orgId, userName);
            }
        }
    }
}
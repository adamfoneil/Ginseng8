using Ginseng.Models;
using Ginseng.Mvc.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
    public class TeamModel : DashboardPageModel
    {
        public TeamModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true, Name = "Id")]
        public int FilterUserId { get; set; }

        public IEnumerable<OrganizationUser> Users { get; set; }

        public Dictionary<int, DefaultActivity> UserIdColumns { get; set; }

        protected override Func<ClosedWorkItemsResult, int> ClosedItemGrouping => (item) => item.DeveloperUserId ?? 0;

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            Users = await new MyOrgUsers()
            {
                OrgId = OrgId,
                TeamId = CurrentOrgUser.CurrentTeamId,
                AppId = CurrentOrgUser.EffectiveAppId,
                HasWorkItems = true,
                IsEnabled = true
            }.ExecuteAsync(connection);

            UserIdColumns = Users.ToDictionary(row => row.UserId, row =>
            {
                int responsibilityId = row.Responsibilities;
                // if you have dev and biz responsibility, then assume dev
                if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;
                return new DefaultActivity()
                {
                    ActivityId = row.DefaultActivityId ?? CurrentOrg.DeveloperActivityId ?? 0,
                    UserIdColumn = Responsibility.WorkItemColumnName[responsibilityId]
                };
            });
        }

        protected override OpenWorkItems GetQuery()
        {
            return new OpenWorkItems(QueryTraces)
            {
                OrgId = OrgId,
                TeamId = CurrentOrgUser.CurrentTeamId,
                AppId = CurrentOrgUser.EffectiveAppId,
                LabelId = LabelId,
                AssignedUserId = FilterUserId
            };
        }
    }
}
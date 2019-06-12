using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class AppsModel : DashboardPageModel
    {
        public AppsModel(IConfiguration config) : base(config)
        {
            ShowExcelDownload = false;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FilterIsActive { get; set; } = true;

        public IEnumerable<AppInfoResult> AppInfo { get; set; }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            if (Id.HasValue)
            {
                //AppInfo = await new AppInfo() { OrgId = OrgId, TeamId = CurrentOrgUser.CurrentTeamId, Id =  }.ExecuteAsync(connection);
            }
            else
            {
                AppInfo = await new AppInfo() { OrgId = OrgId, TeamId = CurrentOrgUser.CurrentTeamId, IsActive = FilterIsActive }.ExecuteAsync(connection);
            }
        }

        protected override OpenWorkItems GetQuery()
        {
            if (Id.HasValue)
            {
                return new OpenWorkItems(QueryTraces)
                {
                    OrgId = OrgId,                    
                    LabelId = LabelId,                    
                    AppId = Id
                };
            }

            return null;

        }
    }
}
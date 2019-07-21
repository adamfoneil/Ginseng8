using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Ginseng.Mvc.Pages.Work
{
    [Authorize]
    public class IndexModel : DashboardPageModel
    {
        public IndexModel(IConfiguration config) : base(config)
        {
            ShowLabelFilter = false;
        }

        public IEnumerable<OpenWorkItemsResult> HungWorkItems { get; private set; }

        public IEnumerable<OpenWorkItemsResult> ImpededItems
        {
            get { return HungWorkItems.Where(wi => wi.HasImpediment); }
        }

        public IEnumerable<OpenWorkItemsResult> PausedItems
        {
            get { return HungWorkItems.Where(wi => wi.IsPaused()); }
        }

        public IEnumerable<OpenWorkItemsResult> StoppedItems
        {
            get { return HungWorkItems.Where(wi => wi.IsStopped()); }
        }

        protected override void OnGetInternal(SqlConnection connection)
        {
            base.OnGetInternal(connection);
            HungWorkItems = WorkItems.Where(wi => wi.IsHung).ToArray();
        }

        protected override OpenWorkItems GetQuery()
        {
            return new OpenWorkItems(QueryTraces)
            {
                OrgId = OrgId,
                AppId = CurrentOrgUser.CurrentAppId,
                TeamId = CurrentOrgUser.CurrentTeamId
            };
        }
    }
}
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
        private IEnumerable<OpenWorkItemsResult> _workItems;

        public IndexModel(IConfiguration config) : base(config)
        {
            ShowLabelFilter = false;
        }

        public IEnumerable<OpenWorkItemsResult> HungWorkItems
        {
            get { return _workItems; }
        }

        public IEnumerable<OpenWorkItemsResult> ImpededItems
        {
            get { return _workItems.Where(wi => wi.HasImpediment); }
        }

        public IEnumerable<OpenWorkItemsResult> PausedItems
        {
            get { return _workItems.Where(wi => wi.IsPaused()); }
        }

        public IEnumerable<OpenWorkItemsResult> StoppedItems
        {
            get { return _workItems.Where(wi => wi.IsStopped()); }
        }

        protected override void OnGetInternal(SqlConnection connection)
        {
            base.OnGetInternal(connection);
            _workItems = WorkItems.Where(wi => wi.HasImpediment || wi.IsPaused() || wi.IsStopped()).ToArray();
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
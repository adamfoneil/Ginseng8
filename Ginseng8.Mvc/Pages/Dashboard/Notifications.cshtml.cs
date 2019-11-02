using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class NotificationsModel : DashboardPageModel
    {
        public NotificationsModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<EventSubscription> EventSubscriptions { get; set; }
        public IEnumerable<LabelSubscription> LabelSubscriptions { get; set; }        
        public ILookup<int, AppNotificationsResult> Notifications { get; set; } // keyed to WorkItemId

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            // let's show the reasons notifications are appearing on this page
            EventSubscriptions = await new MyEventSubscriptions() { OrgId = OrgId, UserId = UserId, InApp = true }.ExecuteAsync(connection);
            LabelSubscriptions = await new MyLabelSubscriptions() { OrgId = OrgId, UserId = UserId, InApp = true }.ExecuteAsync(connection);

            // we'll need to include the notification content in the page so we can tell why a work item is appearing, and also so the notification can be cleared.
            // there can be multiple notifications for a single work item, so I use an ILookup to consolidate multiple notifications
            Notifications = (await new AppNotifications() { OrgId = OrgId, SendTo = User.Identity.Name }.ExecuteAsync(connection)).ToLookup(row => row.WorkItemId);
        }

        protected override OpenWorkItems GetQuery()
        {
            return new OpenWorkItems()
            {
                OrgId = OrgId,
                InMyNotifications = true,
                NotifyUserName = User.Identity.Name
            };
        }
    }
}

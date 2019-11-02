using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public IEnumerable<ActivitySubscription> ActivitySubscriptions { get; set; }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            EventSubscriptions = await new MyEventSubscriptions() { OrgId = OrgId, UserId = UserId, InApp = true }.ExecuteAsync(connection);
            LabelSubscriptions = await new MyLabelSubscriptions() { OrgId = OrgId, UserId = UserId, InApp = true }.ExecuteAsync(connection);
            ActivitySubscriptions = await new MyHandOffActivities() { OrgId = OrgId, UserId = UserId, InApp = true }.ExecuteAsync(connection);
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

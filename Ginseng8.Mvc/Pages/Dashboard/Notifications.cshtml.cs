using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class NotificationsModel : DashboardPageModel
    {
        public NotificationsModel(IConfiguration config) : base(config)
        {
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

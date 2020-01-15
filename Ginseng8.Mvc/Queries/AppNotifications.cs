using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
    public class AppNotificationsResult
    {
        public int WorkItemId { get; set; }
        public int NotificationId { get; set; }
        public string HtmlBody { get; set; }
    }

    public class AppNotifications : Query<AppNotificationsResult>
    {
        public AppNotifications() : base(@"SELECT * FROM dbo.FnAppNotifications(@orgId, @sendTo)")
        {
        }

        public int OrgId { get; set; }
        public string SendTo { get; set; }
    }
}

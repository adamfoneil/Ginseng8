using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages
{
    public class NotificationsModel : AppPageModel
    {
        public NotificationsModel(IConfiguration config) : base(config)
        {
        }

        public int TimeZoneOffset { get; set; }
        public bool AdjustDST { get; set; }
        public IEnumerable<PendingNotificationResult> MyNotifications { get; set; }

        public async Task OnGetAsync()
        {
            TimeZoneOffset = CurrentUser.TimeZoneOffset;
            AdjustDST = CurrentUser.AdjustForDaylightSaving;

            using (var cn = Data.GetConnection())
            {
                MyNotifications = await new PendingNotifications() { Method = DeliveryMethod.App, SendTo = User.Identity.Name }.ExecuteAsync(cn);
            }
        }
    }
}

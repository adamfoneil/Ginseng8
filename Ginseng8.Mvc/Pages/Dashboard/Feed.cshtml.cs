using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class FeedModel : AppPageModel
	{
		public FeedModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Event> Events { get; set; }
		public Dictionary<int, EventSubscription> Subscriptions { get; set; }
		public IEnumerable<EventLogsResult> EventLogs { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				Events = await new Events().ExecuteAsync(cn);
				var subs = await new MyEventSubscriptions() { OrgId = OrgId, UserId = UserId, AppId = CurrentOrgUser.CurrentAppId ?? 0 }.ExecuteAsync(cn);
				Subscriptions = subs.ToDictionary(row => row.EventId);
				var eventIds = subs.Where(s => s.Visible).Select(es => es.EventId).ToArray();
				EventLogs = await new EventLogs() { EventIds = eventIds, OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId ?? 0 }.ExecuteAsync(cn);
			}
		}
	}
}
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class HandOffModel : AppPageModel
	{
		public HandOffModel(IConfiguration config) : base(config)
		{
		}

		public OpenWorkItemsResult WorkItem { get; set; }
		public HandOff HandOff { get; set; }
		public Activity FromActivity { get; set; }
		public Activity ToActivity { get; set; }

		public async Task OnGetAsync(int id, int activityId)
		{
			WorkItem = await FindWorkItemResultAsync(id);

			if (WorkItem.ActivityId != 0)
			{
				FromActivity = await Data.FindAsync<Activity>(WorkItem.ActivityId);
			}

			ToActivity = await Data.FindAsync<Activity>(activityId);

			bool isForward = (FromActivity != null) ?
				ToActivity.Order > FromActivity.Order :
				true;

			HandOff = new HandOff()
			{
				WorkItemId = WorkItem.Id,
				FromUserId = UserId,
				FromActivityId = WorkItem.ActivityId,
				ToActivityId = activityId,
				IsForward = isForward
			};
		}
	}
}
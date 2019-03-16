using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
	public class WorkItemCardView : IUserInfo
	{
		public OpenWorkItemsResult WorkItem { get; set; }
		public IEnumerable<Label> SelectedLabels { get; set; }
		public CommonDropdowns Dropdowns { get; set; }

		public string HandOffButtonText()
		{
			if (WorkItem.ActivityId != 0)
			{
				return WorkItem.AssignedActivityAndUser();
			}
			else
			{
				return "I'll take this";
			}
		}

		public IEnumerable<ActivityOption> GetActivityOptions()
		{
			if (WorkItem.AssignedUserId.HasValue)
			{
				int currentOrder = WorkItem.ActivityOrder ?? 0;
				return Dropdowns.Activities.Where(a => a.Id != WorkItem.ActivityId).Select(a => new ActivityOption(WorkItem.Number, a, a.Order > currentOrder) { IsHandOff = true });
			}
			else
			{
				// starting activities are all considered forward, and there's no hand-off required
				return Dropdowns.Activities.Where(a => a.AllowStart).Select(a => new ActivityOption(WorkItem.Number, a, true));
			}
		}

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public DateTime LocalTime { get; set; }
	}
}
using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
	public class WorkItemCardView : IUserInfo
	{
		public string AccordionElement { get; set; } = "accordion";
		public OpenWorkItemsResult WorkItem { get; set; }
		public IEnumerable<Label> SelectedLabels { get; set; }
		public CommonDropdowns Dropdowns { get; set; }
		public IEnumerable<Comment> Comments { get; set; }
		public bool ShowDetailsButton { get; set; } = true;
        public IEnumerable<SelectListItem> AssignToUsers { get; set; }
        public WorkItemTitleViewField TitleViewField { get; set; } = WorkItemTitleViewField.Project;
        public bool ShowPins { get; set; }
        public bool IsPinned { get; set; }

        public string PinnedClassName()
        {
            return (IsPinned) ? PinnedWorkItem.PinnedIcon : PinnedWorkItem.UnpinnedIcon;
        }

        /// <summary>
        /// Indicates whether ajax calls are made to update work items when fields are changed in the UI.
        /// Set to false when used from _InsertItem.cshtml because it's a new item that doesn't exist yet
        /// </summary>
        public bool UpdatesEnabled { get; set; } = true;

		public string HandOffButtonText()
		{
			if (WorkItem.ActivityId != 0 || WorkItem.DeveloperUserId.HasValue)
			{
				return WorkItem.ActivityStatus();
			}
			else
			{
				return "I'll start this";
			}
		}

		public IEnumerable<ActivityOption> GetActivityOptions()
		{
			if (WorkItem.AssignedUserId.HasValue && WorkItem.ActivityId != 0)
			{
				// assigned person may move hand off to another activity
				int currentOrder = WorkItem.ActivityOrder ?? 0;
				return Dropdowns.Activities.Where(a => a.Id != WorkItem.ActivityId).Select(a => new ActivityOption(WorkItem.Number, a, a.Order > currentOrder) { IsHandOff = true });
			}
			else
			{				
				if (WorkItem.IsPaused())
				{
					// no one assigned (item is paused), so anyone can resume this activity, but there's no choice of activity
					return Enumerable.Empty<ActivityOption>();
				}
				else
				{
					// starting activities are all considered forward, and there's no hand-off required (will call JS self-start-activity event)
					return Dropdowns.Activities.Where(a => a.AllowStart).Select(a => new ActivityOption(WorkItem.Number, a, true));
				}				
			}
		}

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public DateTime LocalTime { get; set; }
	}
}

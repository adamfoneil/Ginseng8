using System;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;

namespace Ginseng.Mvc.ViewModels
{
	public class WorkItemCardView : IUserInfo
	{
		public AllWorkItemsResult WorkItem { get; set; }
		public CommonDropdowns Dropdowns { get; set; }

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public DateTime LocalTime { get; set; }
	}
}
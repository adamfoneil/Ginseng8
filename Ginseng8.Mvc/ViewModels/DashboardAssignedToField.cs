using System;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;

namespace Ginseng.Mvc.ViewModels
{
	public class DashboardAssignedToField : IUserInfo
	{
		public DashboardAssignedToField()
		{
		}

		public DashboardAssignedToField(IUserInfo userInfo)
		{
			UserId = userInfo.UserId;
			OrgId = userInfo.OrgId;
			LocalTime = userInfo.LocalTime;
		}

		public AllWorkItemsResult Item { get; set; }

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public DateTime LocalTime { get; set; }
	}
}
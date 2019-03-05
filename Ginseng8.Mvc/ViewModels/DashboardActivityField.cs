using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class DashboardActivityField : IUserInfo
	{
		public DashboardActivityField()
		{
		}

		public DashboardActivityField(IUserInfo userInfo)
		{
			UserId = userInfo.UserId;
			OrgId = userInfo.OrgId;
			LocalTime = userInfo.LocalTime;
		}

		public AllWorkItemsResult Item { get; set; }
		public IEnumerable<Activity> MyActivities { get; set; }
		public IEnumerable<Activity> AllActivities { get; set; }

		public IEnumerable<Activity> GetActivities()
		{
			return (Item.ActivityId.HasValue) ? AllActivities : MyActivities;
		}

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public DateTime LocalTime { get; set; }
	}
}
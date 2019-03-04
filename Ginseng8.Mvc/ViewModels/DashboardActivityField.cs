﻿using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class DashboardActivityField : IUserInfo
	{

		public AllWorkItemsResult Item { get; set; }
		public IEnumerable<Activity> Activities { get; set; }
		public IEnumerable<CloseReason> CloseReasons { get; set; }

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public DateTime LocalTime { get; set; }
	}
}
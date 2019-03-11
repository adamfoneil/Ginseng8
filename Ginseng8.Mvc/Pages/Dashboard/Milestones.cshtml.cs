﻿using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Dashboard
{
	[Authorize]
	public class MilestonesModel : DashboardPageModel
	{
		public MilestonesModel(IConfiguration config) : base(config)
		{
		}

		protected override AllWorkItems GetQuery()
		{
			return new AllWorkItems()
			{
				OrgId = Data.CurrentOrg.Id,
				HasMilestone = true
			};
		}
	}
}
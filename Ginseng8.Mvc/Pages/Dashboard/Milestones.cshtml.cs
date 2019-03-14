using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Dashboard
{
	[Authorize]
	public class MilestonesModel : DashboardPageModel
	{
		public MilestonesModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty(SupportsGet = true)]
		public int? MilestoneId { get; set; }

		public string ActiveStyle(int milestoneId)
		{
			return (milestoneId == MilestoneId) ? "active" : string.Empty;
		}		

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = Data.CurrentOrg.Id,
				HasMilestone = true,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};			
		}
	}
}
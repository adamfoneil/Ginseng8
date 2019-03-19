using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

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

		public async Task<IActionResult> OnPostCreate(Milestone record)
		{
			record.OrganizationId = OrgId;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Dashboard/Milestones");
		}
	}
}
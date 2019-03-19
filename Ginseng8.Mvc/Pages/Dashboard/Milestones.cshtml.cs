using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

		public IEnumerable<OpenWorkItemsResult> EmptyMilestones { get; set; }
		public IEnumerable<OpenWorkItemsResult> BacklogItems { get; set; }

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				HasMilestone = true,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			var emptyMilestones = await new Milestones() { OrgId = OrgId, HasWorkItems = false }.ExecuteAsync(connection);
			EmptyMilestones = emptyMilestones.Select(ms => new OpenWorkItemsResult()
			{
				MilestoneId = ms.Id,
				MilestoneDate = ms.Date,
				MilestoneDaysAway = ms.DaysAway,
				MilestoneName = ms.Name
			});

			BacklogItems = await new OpenWorkItems() { OrgId = OrgId, HasMilestone = false }.ExecuteAsync(connection);
		}

		public async Task<IActionResult> OnPostCreate(Milestone record)
		{
			record.OrganizationId = OrgId;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Dashboard/Milestones");
		}
	}
}
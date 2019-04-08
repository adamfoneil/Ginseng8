using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Roadmap
{
	public class IndexModel : AppPageModel
	{
		public IndexModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Milestone> Milestones { get; set; }
		public IEnumerable<Application> Applications { get; set; }
		public Dictionary<RoadmapCell, AppMilestone> AppMilestones { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				Milestones = await new Milestones() { OrgId = OrgId }.ExecuteAsync(cn);
				Applications = await new Applications() { OrgId = OrgId, IsActive = true, Id = CurrentOrgUser.CurrentAppId }.ExecuteAsync(cn);

				var cells = await new AppMilestones() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(cn);
				AppMilestones = cells.ToDictionary(row => new RoadmapCell(row.ApplicationId, row.MilestoneId));
			}
		}
	}
}
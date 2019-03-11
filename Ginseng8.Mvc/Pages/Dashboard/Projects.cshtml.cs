using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class ProjectsModel : DashboardPageModel
	{
		public ProjectsModel(IConfiguration config) : base(config)
		{
		}

		protected override AllWorkItems GetQuery()
		{
			return new AllWorkItems() { OrgId = OrgId };
		}
	}
}
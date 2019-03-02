using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class ProjectsModel : AppPageModel
	{
		public ProjectsModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
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
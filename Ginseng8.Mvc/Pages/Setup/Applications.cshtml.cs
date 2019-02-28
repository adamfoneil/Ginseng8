using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
	public class ApplicationsModel : AppPageModel
	{
		public ApplicationsModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
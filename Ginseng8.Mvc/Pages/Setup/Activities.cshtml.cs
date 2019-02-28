using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
	public class ActivitiesModel : AppPageModel
	{
		public ActivitiesModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
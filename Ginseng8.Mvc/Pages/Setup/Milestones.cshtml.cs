using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
	public class MilestonesModel : AppPageModel
	{
		public MilestonesModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
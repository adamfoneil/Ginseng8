using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class FeedModel : AppPageModel
	{
		public FeedModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
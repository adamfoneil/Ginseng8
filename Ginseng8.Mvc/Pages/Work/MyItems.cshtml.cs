using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class MyWorkModel : AppPageModel
	{
		public MyWorkModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
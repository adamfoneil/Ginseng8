using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class AllItemsModel : AppPageModel
	{
		public AllItemsModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
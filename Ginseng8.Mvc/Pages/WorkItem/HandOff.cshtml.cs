using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class HandOffModel : AppPageModel
	{
		public HandOffModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
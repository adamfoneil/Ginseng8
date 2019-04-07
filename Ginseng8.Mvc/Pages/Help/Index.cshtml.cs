using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Help
{
	public class IndexModel : AppPageModel
	{
		public IndexModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
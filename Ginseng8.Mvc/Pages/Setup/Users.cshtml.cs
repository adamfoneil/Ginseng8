using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
	public class UsersModel : AppPageModel
	{
		public UsersModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
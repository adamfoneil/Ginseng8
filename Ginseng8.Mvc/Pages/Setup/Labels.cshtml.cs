using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
	public class LabelsModel : AppPageModel
	{
		public LabelsModel(IConfiguration config) : base(config)
		{
		}

		public void OnGet()
		{
		}
	}
}
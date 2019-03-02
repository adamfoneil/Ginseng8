using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	public class CreateItemModel : AppPageModel
	{
		public CreateItemModel(IConfiguration config) : base(config)
		{
		}

		public SelectList AppSelect { get; set; }
		public SelectList ProjectSelect { get; set; }

		public int ApplicationId { get; set; }
		public int? ProjectId { get; set; }
		public string Title { get; set; }

		public void OnGet()
		{
		}
	}
}
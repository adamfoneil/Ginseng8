using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.ViewComponents
{
	public class NewItemViewComponent : ViewComponent
	{
		private readonly IConfiguration _config;

		public NewItemViewComponent(IConfiguration config)
		{
			_config = config;
		}

		public SelectList AppSelect { get; set; }
		public SelectList ProjectSelect { get; set; }

		public int ApplicationId { get; set; }
		public int? ProjectId { get; set; }
		public string Title { get; set; }

		public async Task<IViewComponentResult> InvokeAsync()
		{
			return View();
		}
	}
}
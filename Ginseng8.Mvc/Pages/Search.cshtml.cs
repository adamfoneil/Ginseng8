using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages
{
	public class SearchModel : AppPageModel
	{
		public SearchModel(IConfiguration config) : base(config)
		{
		}

		public async Task OnGetAsync(string query)
		{
		}
	}
}
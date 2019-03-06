using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class EditModel : AppPageModel
	{
		public EditModel(IConfiguration config) : base(config)
		{
		}

		public Models.WorkItem WorkItem { get; set; }
		
		public async Task OnGetAsync(int id)
		{
			WorkItem = await Data.FindWhereAsync<Models.WorkItem>(new { OrganizationId = OrgId, Number = id });
		}
	}
}
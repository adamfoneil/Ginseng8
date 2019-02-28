using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	[Authorize]
	public class OrganizationModel : AppPageModel
	{				
		public OrganizationModel(IConfiguration config) : base(config)
		{			
		}

		public async Task OnGetAsync()
		{			
			
		}
	}
}
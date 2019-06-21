using Ginseng.Mvc.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
    [Authorize]
    [OrgNotRequired]
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
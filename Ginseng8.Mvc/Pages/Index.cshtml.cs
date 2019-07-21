using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages
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
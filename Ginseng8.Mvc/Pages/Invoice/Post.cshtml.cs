using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Invoice
{
    public class PostModel : AppPageModel
    {
        public PostModel(IConfiguration config) : base(config)
        {
        }

        public void OnGet()
        {
        }
    }
}
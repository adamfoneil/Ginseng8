using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Hours
{
    public class PostInvoiceModel : AppPageModel
    {
        public PostInvoiceModel(IConfiguration config) : base(config)
        {
        }

        public void OnGet()
        {
        }
    }
}
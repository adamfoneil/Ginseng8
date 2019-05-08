using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Invoice
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
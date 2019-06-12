using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class AppsModel : DashboardPageModel
    {
        public AppsModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        public async Task OnGetAsync()
        {
        }

        protected override OpenWorkItems GetQuery()
        {
            if (Id.HasValue)
            {
                return new OpenWorkItems(QueryTraces)
                {
                    OrgId = OrgId,                    
                    LabelId = LabelId,                    
                    AppId = Id
                };
            }

            return null;

        }
    }
}
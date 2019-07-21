using Ginseng.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.EmailContent
{
    public class JoinRequestModel : AppPageModel
    {
        public JoinRequestModel(IConfiguration config) : base(config)
        {
        }

        public OrganizationUser OrgUser { get; set; }

        public async Task OnGetAsync(int id)
        {
            OrgUser = await Data.FindAsync<OrganizationUser>(id);
        }
    }
}
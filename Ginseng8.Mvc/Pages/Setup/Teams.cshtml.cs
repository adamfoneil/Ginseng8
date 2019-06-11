using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class TeamsModel : AppPageModel
    {
        public TeamsModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<Team> Teams { get; set; }

        public async Task OnGetAsync(bool isActive = true)
        {
            using (var cn = Data.GetConnection())
            {
                Teams = await new Teams() { OrgId = OrgId, IsActive = isActive }.ExecuteAsync(cn);
            }
        }

        public async Task<RedirectResult> OnPostSaveAsync(Team record)
        {
            record.OrganizationId = OrgId;
            await Data.TrySaveAsync(record);
            return Redirect("/Setup/Teams");
        }

        public async Task<RedirectResult> OnPostDeleteAsync(int id)
        {
            await Data.TryDeleteAsync<Team>(id);
            return Redirect("/Setup/Teams");
        }
    }
}
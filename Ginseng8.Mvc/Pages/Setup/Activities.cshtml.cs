using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class ActivitiesModel : AppPageModel
    {
        public ActivitiesModel(IConfiguration config) : base(config)
        {
        }

        public SelectList ResponsibilitySelect { get; set; }
        public IEnumerable<Activity> Activities { get; set; }
        public IEnumerable<Activity> UserOrder { get; set; }

        public async Task OnGetAsync(bool isActive = true)
        {
            using (var cn = Data.GetConnection())
            {
                Activities = await new Activities() { OrgId = OrgId, IsActive = isActive }.ExecuteAsync(cn);
                ResponsibilitySelect = await new ResponsibilitySelect().ExecuteSelectListAsync(cn);
                UserOrder = await new MyActivityOrder() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(cn);
            }
        }

        public async Task<ActionResult> OnPostSaveAsync(Activity record)
        {
            record.OrganizationId = CurrentOrg.Id;
            await Data.TrySaveAsync(record);
            return RedirectToPage("/Setup/Activities");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            await Data.TryDeleteAsync<Activity>(id);
            return RedirectToPage("/Setup/Activities");
        }
    }
}
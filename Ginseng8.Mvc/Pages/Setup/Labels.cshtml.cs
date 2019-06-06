using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class LabelsModel : AppPageModel
    {
        public LabelsModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<Label> Labels { get; set; }
        public Dictionary<int, LabelSubscription> Subscriptions { get; set; }
        public IEnumerable<Application> AllApps { get; set; }
        public ILookup<int, NewItemAppLabel> NewItemApps { get; set; }

        public async Task OnGetAsync(bool isActive = true)
        {
            using (var cn = Data.GetConnection())
            {
                Labels = await new Labels() { OrgId = OrgId, IsActive = isActive }.ExecuteAsync(cn);

                await new InsertDefaultLabelSubscriptions() { OrgId = OrgId, UserId = UserId, UserName = User.Identity.Name }.ExecuteAsync(cn);

                var subscriptions = await new MyLabelSubscriptions() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(cn);
                Subscriptions = subscriptions.ToDictionary(row => row.LabelId);

                AllApps = await new Applications() { OrgId = OrgId }.ExecuteAsync(cn);

                var newItemAppLabels = await new NewItemAppLabels() { OrgId = OrgId }.ExecuteAsync(cn);
                NewItemApps = newItemAppLabels.ToLookup(row => row.LabelId);                
            }
        }
        
        public AppSelector GetAppSelector(IEnumerable<NewItemAppLabel> apps)
        {
            return new AppSelector()
            {
                Applications = AllApps.Select(app => new Application()
                {
                    Id = app.Id,
                    Name = app.Name,
                    Selected = apps.Contains()
                })
            };
        }

        public async Task<ActionResult> OnPostSave(Label label)
        {
            label.OrganizationId = OrgId;
            await Data.TrySaveAsync(label);
            return RedirectToPage("/Setup/Labels");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            await Data.TryDeleteAsync<Label>(id);
            return RedirectToPage("/Setup/Labels");
        }
    }
}
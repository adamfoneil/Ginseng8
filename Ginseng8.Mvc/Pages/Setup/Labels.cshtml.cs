using Ginseng.Models;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
        public ILookup<int, Application> NewItemApps { get; set; }

        public async Task OnGetAsync(bool isActive = true)
        {
            using (var cn = Data.GetConnection())
            {
                Labels = await new Labels() { OrgId = OrgId, IsActive = isActive }.ExecuteAsync(cn);

                await new InsertDefaultLabelSubscriptions() { OrgId = OrgId, UserId = UserId, UserName = User.Identity.Name }.ExecuteAsync(cn);

                var subscriptions = await new MyLabelSubscriptions() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(cn);
                Subscriptions = subscriptions.ToDictionary(row => row.LabelId);

                AllApps = await new Applications() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);

                var newItemAppLabels = await new NewItemAppLabelsInUse() { OrgId = OrgId }.ExecuteAsync(cn);
                NewItemApps = newItemAppLabels.ToLookup(row => row.LabelId);
            }
        }

        public MultiSelector<ISelectable> GetAppSelector(int labelId)
        {
            var apps = NewItemApps[labelId];
            return new MultiSelector<ISelectable>()
            {
                Prompt = "Apps:",
                PrimaryFieldName = "ApplicationId",
                RelatedFieldName = "LabelId",
                PostUrl = "/Update/NewItemAppLabel",
                RelatedId = labelId,
                Items = AllApps.Select(t => new Team()
                {
                    Id = t.Id,
                    Name = t.Name,
                    Selected = apps.Contains(t)
                })
            };
        }

        public async Task<ActionResult> OnPostSave(Label label)
        {
            label.OrganizationId = OrgId;
            await Data.TrySaveAsync(label);
            return Redirect($"/Setup/Labels#{label.Id}");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            await Data.TryDeleteAsync<Label>(id);
            return Redirect("/Setup/Labels");
        }
    }
}
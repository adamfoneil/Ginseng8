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
        public IEnumerable<Team> AllTeams { get; set; }
        public ILookup<int, Team> NewItemTeams { get; set; }

        public async Task OnGetAsync(bool isActive = true)
        {
            using (var cn = Data.GetConnection())
            {
                Labels = await new Labels() { OrgId = OrgId, IsActive = isActive }.ExecuteAsync(cn);

                await new InsertDefaultLabelSubscriptions() { OrgId = OrgId, UserId = UserId, UserName = User.Identity.Name }.ExecuteAsync(cn);

                var subscriptions = await new MyLabelSubscriptions() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(cn);
                Subscriptions = subscriptions.ToDictionary(row => row.LabelId);

                AllTeams = await new Teams() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);

                var labelsInUse = await new TeamLabelsInUse() { OrgId = OrgId }.ExecuteAsync(cn);
                NewItemTeams = labelsInUse.ToLookup(row => row.LabelId);
            }
        }

        public MultiSelector<ISelectable> GetTeamSelector(int labelId)
        {
            var apps = NewItemTeams[labelId];
            return new MultiSelector<ISelectable>()
            {
                PrimaryFieldName = "TeamId",
                RelatedFieldName = "LabelId",
                PostUrl = "/Update/TeamLabel",
                RelatedId = labelId,
                Items = AllTeams.Select(t => new Team()
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
            return RedirectToPage("/Setup/Labels");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            await Data.TryDeleteAsync<Label>(id);
            return RedirectToPage("/Setup/Labels");
        }
    }
}
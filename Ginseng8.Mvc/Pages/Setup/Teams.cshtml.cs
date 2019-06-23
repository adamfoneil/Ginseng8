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
    public class TeamsModel : AppPageModel
    {
        public TeamsModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public bool IsActive { get; set; } = true;

        public IEnumerable<Team> Teams { get; set; }
        public IEnumerable<Label> AllLabels { get; set; }
        public ILookup<int, Label> TeamLabels { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                Teams = await new Teams() { OrgId = OrgId, IsActive = IsActive }.ExecuteAsync(cn);

                AllLabels = await new Labels() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);

                var teamLabels = await new TeamLabelsInUseByTeam() { OrgId = OrgId }.ExecuteAsync(cn);
                TeamLabels = teamLabels.ToLookup(row => row.TeamId);
            }
        }

        public MultiSelector<ISelectable> GetLabelSelector(int teamId)
        {
            var labels = TeamLabels[teamId];
            return new MultiSelector<ISelectable>()
            {
                IdPrefix = "teams",
                Prompt = "Labels:",
                PrimaryFieldName = "LabelId",
                RelatedFieldName = "TeamId",
                PostUrl = "/Update/TeamLabel",
                RelatedId = teamId,
                Items = AllLabels.Select(lbl => new Label()
                {
                    Id = lbl.Id,
                    Name = lbl.Name,
                    Selected = labels.Contains(lbl),
                    ForeColor = lbl.ForeColor,
                    BackColor = lbl.BackColor
                })
            };
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
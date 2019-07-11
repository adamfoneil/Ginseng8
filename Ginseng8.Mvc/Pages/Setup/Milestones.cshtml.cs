using Dapper;
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
    public class MilestonesModel : AppPageModel
    {
        public MilestonesModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int TeamId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AppId { get; set; }

        public SelectList TeamSelect { get; set; }
        public SelectList AppSelect { get; set; }
        public IEnumerable<Milestone> Milestones { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                TeamSelect = await new TeamSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, TeamId);
                AppSelect = await new AppSelect() { OrgId = OrgId, TeamId = TeamId }.ExecuteSelectListAsync(cn, AppId);
                Milestones = await new Milestones() { OrgId = OrgId, TeamId = TeamId, AppId = AppId, IsSelectable = null, MinDate = null }.ExecuteAsync(cn);
            }
        }

        public async Task<ActionResult> OnPostSave(Milestone record)
        {
            record.OrganizationId = OrgId;
            await Data.TrySaveAsync(record);
            return Redirect($"/Setup/Milestones?teamId={record.TeamId}&appId={record.ApplicationId}");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            using (var cn = Data.GetConnection())
            {
                await cn.ExecuteAsync(
                    @"UPDATE [wi] SET [MilestoneId]=NULL FROM [dbo].[WorkItem] [wi] 
                    WHERE [MilestoneId]=@msId AND [OrganizationId]=@orgId", new { msId = id, orgId = OrgId });

                await cn.ExecuteAsync(
                    @"DELETE [dm] FROM [dbo].[DeveloperMilestone] [dm] 
                    INNER JOIN [dbo].[Milestone] [ms] ON [dm].[MilestoneId]=[ms].[Id] 
                    INNER JOIN [dbo].[Team] [t] ON [ms].[TeamId]=[t].[Id]
                    WHERE [dm].[MilestoneId]=@msId AND [t].[OrganizationId]=@orgId", new { msId = id, orgId = OrgId });

                var ms = await Data.FindAsync<Milestone>(cn, id);
                await Data.TryDeleteAsync<Milestone>(cn, id);
                return Redirect($"/Setup/Milestones?teamId={ms.TeamId}&appId={ms.ApplicationId}");
            }                            
        }
    }
}
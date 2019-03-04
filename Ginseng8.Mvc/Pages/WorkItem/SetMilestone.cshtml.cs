using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class SetMilestoneModel : AppPageModel
	{
		public SetMilestoneModel(IConfiguration config) : base(config)
		{
		}

		public DashboardMilestoneField Field { get; set; }

		public async Task OnPostAsync(int number, int milestoneId)
		{
			using (var cn = Data.GetConnection())
			{
				var item = await Data.FindWhereAsync<Models.WorkItem>(cn, new { OrganizationId = Data.CurrentOrg.Id, Number = number });

				Milestone ms = null;
				if (milestoneId != 0)
				{
					ms = await Data.FindWhereAsync<Milestone>(cn, new { OrganizationId = Data.CurrentOrg.Id, Id = milestoneId });
				}				

				if (item != null)
				{
					if (ms != null)
					{
						item.MilestoneId = ms.Id;
					}
					else
					{
						item.MilestoneId = null;
					}
					 
					await Data.TrySaveAsync(cn, item);
				}

				Field = new DashboardMilestoneField()
				{
					Item = await new AllWorkItems() { OrgId = Data.CurrentOrg.Id, Number = number }.ExecuteSingleAsync(cn),
					Milestones = await new Milestones() { OrgId = Data.CurrentOrg.Id }.ExecuteAsync(cn)
				};
			}
		}
	}
}
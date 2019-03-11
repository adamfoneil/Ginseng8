using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class SetActivityModel : AppPageModel
	{
		public DashboardActivityField Field { get; set; }

		public SetActivityModel(IConfiguration config) : base(config)
		{
		}

		/// <summary>
		/// Sets the Activity on a WorkItem and assigns the current user according to the Activity's responsibility
		/// </summary>
		public async Task OnPostAsync(int number, int activityId)
		{
			using (var cn = Data.GetConnection())
			{
				var item = await Data.FindWhereAsync<Models.WorkItem>(cn, new { OrganizationId = OrgId, Number = number });

				Activity act = null;
				if (activityId != 0)
				{
					act = await Data.FindWhereAsync<Activity>(cn, new { OrganizationId = OrgId, Id = activityId });
				}

				if (item != null)
				{
					if (act != null)
					{
						item.ActivityId = act.Id;
						Responsibility.SetWorkItemUserActions[act.ResponsibilityId].Invoke(item, UserId);
					}
					else
					{
						item.ActivityId = null;
					}

					await Data.TrySaveAsync(cn, item);
				}

				Field = new DashboardActivityField()
				{
					Item = await new AllWorkItems() { OrgId = OrgId, Number = number }.ExecuteSingleAsync(cn),
					MyActivities = await new Activities() { OrgId = OrgId, IsActive = true, Responsibilities = CurrentOrgUser.Responsibilities }.ExecuteAsync(cn),
					AllActivities = await new Activities() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn)
				};
			}
		}
	}
}
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class AppActivitiesModel : AppPageModel
	{
		public AppActivitiesModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Application> Applications { get; set; }
		public IEnumerable<Activity> Activities { get; set; }
		public Dictionary<AppActivity, ActivitySubscription> MyActivities { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				Applications = await new Applications() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);
				Activities = await new Activities() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);
				var myActivities = await new MyHandOffActivities() { UserId = UserId, OrgId = OrgId }.ExecuteAsync(cn);
				MyActivities = myActivities.ToDictionary(row => new AppActivity() { ActivityId = row.ActivityId, ApplicationId = row.ApplicationId });
			}
		}

		public async Task<IActionResult> OnPostUpdateSubscriptionAsync(AppActivity appActivity)
		{
			using (var cn = Data.GetConnection())
			{
				var actSub = await Data.FindWhereAsync<ActivitySubscription>(cn, new
				{
					OrganizationId = OrgId,
					UserId,
					appActivity.ApplicationId,
					appActivity.ActivityId
				});

				if (appActivity.IsSelected)
				{
					if (actSub == null)
					{
						actSub = new ActivitySubscription()
						{
							OrganizationId = OrgId,
							UserId = UserId,
							ActivityId = appActivity.ActivityId,
							ApplicationId = appActivity.ApplicationId
						};
					}					
					await Data.TrySaveAsync(actSub);
				}
				else
				{
					await Data.TryDeleteAsync<ActivitySubscription>(actSub.Id);
				}				
			}
				
			return RedirectToPage("AppActivities");
		}
	}

	public class AppActivity
	{
		public int ApplicationId { get; set; }
		public int ActivityId { get; set; }

		/// <summary>
		/// Used to bind the checkbox that toggles the subscription on or orff
		/// </summary>
		public bool IsSelected { get; set; }

		public override bool Equals(object obj)
		{
			var test = obj as AppActivity;
			return (test != null) ?
				(test.ApplicationId == ApplicationId && test.ActivityId == ActivityId) :
				false;
		}

		public override int GetHashCode()
		{
			return (ApplicationId + ActivityId).GetHashCode();
		}
	}
}
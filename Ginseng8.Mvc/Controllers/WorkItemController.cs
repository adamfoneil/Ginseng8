using Ginseng.Models;
using Ginseng.Mvc.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Controllers
{
	[Authorize]
	public class WorkItemController : Controller
	{
		private readonly DataAccess _data;

		public WorkItemController(IConfiguration config)
		{
			_data = new DataAccess(config);
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);
			_data.Initialize(User, TempData);
		}

		[HttpPost]		
		public async Task<RedirectResult> Create(WorkItem workItem, string returnUrl)
		{
			// make sure incoming number has not been set so that SetNumber method works
			workItem.Number = 0;

			// make sure item is part of this org
			workItem.OrganizationId = _data.CurrentOrg.Id;
			workItem.SaveHtml();

			using (var cn = _data.GetConnection())
			{
				await workItem.SetNumberAsync(cn);
				if (await _data.TrySaveAsync(cn, workItem))
				{
					return Redirect(returnUrl + $"#{workItem.Number}");
				}
				else
				{
					// error message should be at the top
					return Redirect(returnUrl);
				}				
			}
		}

		[HttpPost]		
		public async Task<JsonResult> SelfStartActivity([FromForm]int id, [FromForm]int activityId)
		{
			try
			{
				using (var cn = _data.GetConnection())
				{
					var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
					var activity = await _data.FindAsync<Activity>(cn, activityId);
					workItem.ActivityId = activityId;
					Responsibility.SetWorkItemUserActions[activity.ResponsibilityId].Invoke(workItem, _data.CurrentUser.UserId);
					await _data.TrySaveAsync(cn, workItem);
					return Json(new { success = true});
				}
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		[HttpPost]
		public async Task<JsonResult> ResumeActivity([FromForm]int id)
		{
			try
			{
				using (var cn = _data.GetConnection())
				{
					var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
					var activity = await _data.FindAsync<Activity>(cn, workItem.ActivityId.Value);					
					Responsibility.SetWorkItemUserActions[activity.ResponsibilityId].Invoke(workItem, _data.CurrentUser.UserId);
					await _data.TrySaveAsync(cn, workItem);
					return Json(new { success = true });
				}
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		[HttpPost]
		public async Task<JsonResult> UnassignMe([FromForm]int id)
		{
			try
			{
				using (var cn = _data.GetConnection())
				{
					var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
					var activity = await _data.FindAsync<Activity>(cn, workItem.ActivityId.Value);
					Responsibility.ClearWorkItemUserActions[activity.ResponsibilityId].Invoke(workItem);
					await _data.TrySaveAsync(cn, workItem);
					return Json(new { success = true });
				}
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}
	}
}
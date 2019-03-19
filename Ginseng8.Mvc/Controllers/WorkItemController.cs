﻿using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postulate.SqlServer.IntKey;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

		[HttpPost]
		public async Task<JsonResult> SetPriorities()
		{
			try
			{
				string body = await Request.ReadStringAsync();									
				var data = JsonConvert.DeserializeObject<PriorityUpdate>(body);

				using (var cn = _data.GetConnection())
				{
					var workItem = await cn.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, data.Number });
					await UpdateMilestoneAsync(cn, workItem, data.MilestoneId);
					await UpdateAssignedUserAsync(cn, workItem, data.UserId);

					await cn.ExecuteAsync("dbo.UpdateWorkItemPriorities", new
					{
						userName = User.Identity.Name,
						localTime = _data.CurrentUser.LocalTime,
						userId = data.UserId,
						orgId = _data.CurrentOrg.Id,
						milestoneId = data.MilestoneId,
						priorities = data.Items.AsTableValuedParameter("dbo.WorkItemPriority", "Number", "Index")
					}, commandType: CommandType.StoredProcedure);
				}
				return Json(new { success = true });		
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		[HttpPost]
		public async Task<JsonResult> SetUser()
		{
			try
			{
				string body = await Request.ReadStringAsync();
				var data = JsonConvert.DeserializeObject<UserUpdate>(body);

				using (var cn = _data.GetConnection())
				{
					var workItem = await cn.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, data.Number });
					await UpdateAssignedUserAsync(cn, workItem, data.UserId);
				}
					
				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		private async Task UpdateAssignedUserAsync(SqlConnection cn, WorkItem workItem, int userId)
		{
			// get the responsibility from the work item activity (if set)
			// or fall back to the user profile
			int responsibilityId = (workItem.ActivityId.HasValue) ?
				(await cn.FindAsync<Activity>(workItem.ActivityId.Value)).ResponsibilityId :
				(await cn.FindWhereAsync<OrganizationUser>(new { OrganizationId = _data.CurrentOrg.Id, UserId = userId }) ?? new OrganizationUser()).Responsibilities;

			// if user has both biz and dev responsibility, assume dev
			if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;
						
			if (userId != 0)
			{
				Responsibility.SetWorkItemUserActions[responsibilityId].Invoke(workItem, userId);
			}
			else
			{
				Responsibility.ClearWorkItemUserActions[responsibilityId].Invoke(workItem);
			}

			await cn.UpdateAsync(workItem, _data.CurrentUser, r => r.DeveloperUserId, r => r.BusinessUserId);			
		}

		private async Task UpdateMilestoneAsync(SqlConnection cn, WorkItem workItem, int milestoneId)
		{
			if ((workItem.MilestoneId ?? 0) != milestoneId)
			{
				workItem.MilestoneId = (milestoneId == 0) ? default(int?) : milestoneId;
				await cn.UpdateAsync(workItem, _data.CurrentUser, r => r.MilestoneId);
			}
		}
	}
}
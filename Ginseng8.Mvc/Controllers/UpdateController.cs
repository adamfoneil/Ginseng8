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
	public class UpdateController : Controller
	{
		private readonly DataAccess _data;

		public UpdateController(IConfiguration config)
		{
			_data = new DataAccess(config);			
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);
			_data.Initialize(User, TempData);
		}

		[HttpPost]
		public async Task<JsonResult> WorkItem(WorkItem fields)
		{
			try
			{
				var workItem = await _data.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, fields.Number });
				workItem.ApplicationId = fields.ApplicationId;
				workItem.ProjectId = fields.ProjectId;
				workItem.MilestoneId = fields.MilestoneId;
				workItem.SizeId = fields.SizeId;
				workItem.CloseReasonId = fields.CloseReasonId;
				workItem.ModifiedBy = User.Identity.Name;
				workItem.DateModified = _data.CurrentUser.LocalTime;
				await _data.TryUpdateAsync(workItem, 
					r => r.ApplicationId, r => r.ProjectId, r => r.MilestoneId, r => r.SizeId, r => r.CloseReasonId,
					r => r.ModifiedBy, r => r.DateModified);
				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		[HttpPost]		
		public async Task<JsonResult> WorkItemBody(int number, string htmlBody)
		{
			try
			{
				var workItem = await _data.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, number });
				workItem.HtmlBody = htmlBody;
				workItem.SaveHtml();
				workItem.ModifiedBy = User.Identity.Name;
				workItem.DateModified = _data.CurrentUser.LocalTime;
				await _data.TryUpdateAsync(workItem, r => r.HtmlBody, r => r.TextBody, r => r.ModifiedBy, r => r.DateModified);
				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}
	}
}
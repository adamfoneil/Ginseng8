using Ginseng.Models;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
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
		public async Task<JsonResult> WorkItemBody(int id, string htmlBody)
		{
			var workItem = await _data.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, number = id });
			return await UpdateInnerAsync<WorkItem>(workItem, htmlBody);
		}

		public async Task<JsonResult> ProjectBody(int id, string htmlBody)
		{
			var project = await _data.FindAsync<Project>(id);
			return await UpdateInnerAsync(project, htmlBody);
		}

		private async Task<JsonResult> UpdateInnerAsync<T>(T record, string htmlBody) where T : BaseTable, IBody
		{
			try
			{
				record.HtmlBody = htmlBody;
				record.SaveHtml();
				record.ModifiedBy = User.Identity.Name;
				record.DateModified = _data.CurrentUser.LocalTime;
				await _data.TryUpdateAsync<T>(record, r => r.HtmlBody, r => r.TextBody, r => r.ModifiedBy, r => r.DateModified);
				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		[Route("/Update/CurrentApp/{id}")]
		public async Task<ActionResult> CurrentApp(int id, string returnUrl)
		{
			_data.CurrentOrgUser.CurrentAppId = (id != 0) ? id : default(int?);
			await _data.TryUpdateAsync(_data.CurrentOrgUser, r => r.CurrentAppId);
			return Redirect(returnUrl);
		}
	}
}
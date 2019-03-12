using Ginseng.Models;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
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

		[HttpPost]
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

		public async Task<PartialViewResult> WorkItemLabel(int workItemNumber, int labelId, bool selected)
		{
			using (var cn = _data.GetConnection())
			{
				var workItem = await cn.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, number = workItemNumber });
				if (workItem != null)
				{			
					if (selected)
					{
						var wil = new WorkItemLabel() { WorkItemId = workItem.Id, LabelId = labelId };
						await cn.MergeAsync(wil, _data.CurrentUser);
					}
					else
					{
						var wil = await cn.FindWhereAsync<WorkItemLabel>(new { WorkItemId = workItem.Id, LabelId = labelId });
						if (wil != null) await cn.DeleteAsync<WorkItemLabel>(wil.Id, _data.CurrentUser);
					}

					var results = await new LabelsInUse() { OrgId = _data.CurrentOrg.Id, WorkItemIds = new int[] { workItem.Id } }.ExecuteAsync(cn);

					return PartialView("/Pages/Dashboard/Items/_Labels.cshtml", results);
				}				
			}

			throw new Exception($"Work item number {workItemNumber} not found.");
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
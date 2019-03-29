using Dapper;
using Ginseng.Models;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postulate.SqlServer.IntKey;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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
			return await UpdateInnerAsync(workItem, htmlBody);
		}

		public async Task<JsonResult> ModelClassBody(int id, string htmlBody)
		{
			var mc = await _data.FindAsync<ModelClass>(id);
			return await UpdateInnerAsync(mc, htmlBody);
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
				await _data.TryUpdateAsync(record, r => r.HtmlBody, r => r.TextBody, r => r.ModifiedBy, r => r.DateModified);
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
			if (_data.CurrentOrgUser == null) return Redirect(returnUrl);

			_data.CurrentOrgUser.CurrentAppId = (id != 0) ? id : default(int?);
			await _data.TryUpdateAsync(_data.CurrentOrgUser, r => r.CurrentAppId);
			return Redirect(returnUrl);
		}

		public async Task<ContentResult> ModelClassName(string elementId, string newName)
		{
			int id = IntFromText(elementId);
			var mc = await _data.FindAsync<ModelClass>(id);
			mc.Name = newName;
			await _data.TryUpdateAsync(mc, r => r.Name);
			return Content(newName);
		}

		private int IntFromText(string input)
		{
			var result = Regex.Match(input, @"\d+");
			return Convert.ToInt32(result.Value);
		}

		/// <summary>
		/// Sets model class property order after drag-drop operation in Pages/Data/Index
		/// </summary>		
		[HttpPost]
		public async Task<JsonResult> PropertyOrder()
		{
			try
			{
				string body = await Request.ReadStringAsync();
				var rawRata = JsonConvert.DeserializeObject<OrderedItem[]>(body);

				// for compatibility with table type dbo.WorkItemPriority, I use the Item type, which
				// was originally intended for setting WorkItem priority. I didn't want to create a new table type
				// nor rework dbo.WorkItemPriority, so I simple project the incoming raw data to the Item type
				var data = rawRata.Select(item => new Item() { Index = item.Index, Number = item.Id }); 

				using (var cn = _data.GetConnection())
				{
					await cn.ExecuteAsync("dbo.UpdateModelPropertyOrder", new
					{
						userName = User.Identity.Name,
						localTime = _data.CurrentUser.LocalTime,
						orgId = _data.CurrentOrg.Id,
						propertyOrder = data.AsTableValuedParameter("dbo.WorkItemPriority", "Number", "Index")
					}, commandType: CommandType.StoredProcedure);
				}

				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}
	}
}
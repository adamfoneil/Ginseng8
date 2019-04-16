using Dapper;
using Ginseng.Models;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
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

		public async Task<JsonResult> Project(Project fields)
		{
			try
			{
				using (var cn = _data.GetConnection())
				{
					var project = await _data.FindAsync<Project>(cn, fields.Id);
					project.ApplicationId = fields.ApplicationId;
					project.IsActive = fields.IsActive;
					project.DataModelId = fields.DataModelId;
					await cn.UpdateAsync(project, _data.CurrentUser, r => r.ApplicationId, r => r.IsActive, r => r.DataModelId);
					await project.SyncWorkItemsToProjectAsync(cn);
					return Json(new { success = true });
				}
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		[HttpPost]		
		public async Task<JsonResult> WorkItemBody(int id, string htmlBody)
		{
			var workItem = await _data.FindAsync<WorkItem>(id);
			return await UpdateInnerAsync(workItem, htmlBody);
		}

		public async Task<JsonResult> AppMilestoneBody(int appId, int milestoneId, string htmlBody)
		{
			try
			{
				using (var cn = _data.GetConnection())
				{
					var appMs =
						await cn.FindWhereAsync<AppMilestone>(new { ApplicationId = appId, MilestoneId = milestoneId }) ??
						new AppMilestone() { ApplicationId = appId, MilestoneId = milestoneId };

					appMs.HtmlBody = htmlBody;
					appMs.SaveHtml(cn);

					await cn.SaveAsync(appMs, _data.CurrentUser);
				}
				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}				
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
				record.ModifiedBy = User.Identity.Name;
				record.DateModified = _data.CurrentUser.LocalTime;

				using (var cn = _data.GetConnection())
				{					
					record.SaveHtml(cn);
				}

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
		public async Task<RedirectResult> CurrentApp(int id, string returnUrl)
		{
			if (_data.CurrentOrgUser == null) return Redirect(returnUrl);

			_data.CurrentOrgUser.CurrentAppId = (id != 0) ? id : default(int?);
			await _data.TryUpdateAsync(_data.CurrentOrgUser, r => r.CurrentAppId);
			return Redirect(returnUrl);
		}

		[Route("/Update/CurrentOrg/{id}")]
		public async Task<RedirectResult> CurrentOrg(int id, string returnUrl)
		{
			using (var cn = _data.GetConnection())
			{
				//var myOrgs = await new MyOrgs() { UserId = _data.CurrentUser.UserId }.ExecuteAsync(cn);
			}
				
			_data.CurrentUser.OrganizationId = id;
			await _data.TryUpdateAsync(_data.CurrentUser, r => r.OrganizationId);
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

		public async Task<ContentResult> WorkItemTitle(string elementId, string newTitle)
		{
			int number = IntFromText(elementId);
			var workItem = await _data.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, Number = number });
			workItem.Title = newTitle;
			await _data.TryUpdateAsync(workItem, r => r.Title);
			return Content(newTitle);
		}

		public async Task<ContentResult> ProjectName(string elementId, string newName)
		{
			int projectId = IntFromText(elementId);
			var project = await _data.FindAsync<Project>(projectId);
			if (project.Application.OrganizationId != _data.CurrentOrg.Id) throw new Exception("Application is in a different organization.");
			project.Name = newName;
			await _data.TryUpdateAsync(project, r => r.Name);
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

		public async Task<JsonResult> EventSubscription(int appId, int eventId, bool selected)
		{
			try
			{
				var ev = await _data.FindWhereAsync<EventSubscription>(new
				{
					EventId = eventId,
					OrganizationId = _data.CurrentOrg.Id,
					ApplicationId = appId,
					_data.CurrentUser.UserId
				}) ?? new EventSubscription()
				{
					EventId = eventId,
					OrganizationId = _data.CurrentOrg.Id,
					ApplicationId = appId,
					UserId = _data.CurrentUser.UserId
				};

				ev.Visible = selected;
				await _data.TrySaveAsync(ev);

				return Json(new { success = true });
			}
			catch (Exception exc)
			{
				return Json(new { success = false, message = exc.Message });
			}
		}

		public async Task<PartialViewResult> ToggleNotification(int id, string propertyName, string tableName)
		{
			var getNotification = new Dictionary<string, Func<IDbConnection, Task<INotifyOptions>>>()
			{
				{ nameof(Ginseng.Models.EventSubscription), async (cn) => await cn.FindAsync<EventSubscription>(id) },
				{ nameof(ActivitySubscription), async (cn) => await cn.FindAsync<ActivitySubscription>(id) }
			};

			using (var cn = _data.GetConnection())
			{
				var notification = await getNotification[tableName].Invoke(cn);
				var property = notification.GetType().GetProperty(propertyName);
				bool value = !(bool)property.GetValue(notification);
				
				// it's too bad this doesn't do BaseTable.BeforeUpdate (so user/date stamp not updated), but maybe some day
				await cn.ExecuteAsync($"UPDATE [dbo].[{notification.TableName}] SET [{propertyName}]=@value WHERE [Id]=@id", new { value, id });

				property.SetValue(notification, value);

				return PartialView("/Pages/Shared/_NotifyOptions.cshtml", notification);
			}					
		}

		[HttpPost]
		public async Task<JsonResult> ProjectPriorities()
		{
			try
			{
				string body = await Request.ReadStringAsync();
				var data = JsonConvert.DeserializeObject<ProjectPriorityUpdate>(body);

				using (var cn = _data.GetConnection())
				{
					await cn.ExecuteAsync("dbo.UpdateProjectPriorities", new
					{
						userName = User.Identity.Name,
						localTime = _data.CurrentUser.LocalTime,
						orgId = _data.CurrentOrg.Id,
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
	}
}
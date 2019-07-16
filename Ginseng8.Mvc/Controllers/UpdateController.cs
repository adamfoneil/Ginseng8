using Dapper;
using Ginseng.Models;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Classes;
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

                if (fields.TeamId != workItem.TeamId)
                {
                    // if the team changes, then some other fields must be cleared
                    workItem.ApplicationId = null;
                    workItem.ProjectId = null;
                    workItem.MilestoneId = null;
                }

                string backgroundColor = string.Empty;
                string className = "badge-secondary";                
                bool getColor = (fields.SizeId != workItem.SizeId || workItem.SizeId.HasValue);
                bool showMissingEstimateModifier = !fields.SizeId.HasValue;
                string missingEstimateModifierId = $"mod-{workItem.Number}-{OpenWorkItemsResult.UnestimatedModifier}";

                workItem.TeamId = fields.TeamId;
                workItem.ApplicationId = fields.ApplicationId;
                workItem.ProjectId = fields.ProjectId;

                if (fields.MilestoneId.HasValue)
                {
                    if (fields.MilestoneId > 0)
                    {
                        workItem.MilestoneId = fields.MilestoneId;
                    }
                    else if (fields.MilestoneId < 0)
                    {
                        using (var cn = _data.GetConnection())
                        {
                            var indirectMilestones = new IndirectMilestones();
                            workItem.MilestoneId = await indirectMilestones.Options[fields.MilestoneId.Value].GetMilestoneIdAsync(cn, _data.CurrentUser, _data.CurrentOrg.Id, fields.TeamId);
                        }
                    }
                }
                else
                {
                    workItem.MilestoneId = null;
                }

                workItem.SizeId = fields.SizeId;
                workItem.CloseReasonId = fields.CloseReasonId;
                workItem.ModifiedBy = User.Identity.Name;
                workItem.DateModified = _data.CurrentUser.LocalTime;

                await _data.TryUpdateAsync(workItem,
                    r => r.TeamId, r => r.ApplicationId, r => r.ProjectId, r => r.MilestoneId, r => r.SizeId, r => r.CloseReasonId,
                    r => r.ModifiedBy, r => r.DateModified);

                if (getColor)
                {
                    using (var cn = _data.GetConnection())
                    {
                        var item = await new OpenWorkItems() { Id = workItem.Id, OrgId = _data.CurrentOrg.Id, IsOpen = null }.ExecuteSingleAsync(cn);
                        if (item.SizeId.HasValue)
                        {
                            backgroundColor = HtmlHelpers.WeightedColorToHex(item.ColorGradientPosition);
                            className = string.Empty;
                        }
                        else
                        {
                            backgroundColor = string.Empty;
                            className = "badge-secondary";
                        }                        
                    }
                }
                
                return Json(new { success = true, backgroundColor, className, showMissingEstimateModifier, missingEstimateModifierId });
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
                    project.TeamId = fields.TeamId;
                    project.ApplicationId = fields.ApplicationId;
                    project.IsActive = fields.IsActive;
                    project.DataModelId = fields.DataModelId;
                    project.FreshdeskCompanyId = fields.FreshdeskCompanyId;
                    await cn.UpdateAsync(project, _data.CurrentUser, r => r.TeamId, r => r.ApplicationId, r => r.IsActive, r => r.DataModelId, r => r.FreshdeskCompanyId);
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
                await record.SaveHtmlAsync(_data);

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

        public async Task<ContentResult> TeamLabel(int teamId, int labelId, bool selected)
        {
            using (var cn = _data.GetConnection())
            {
                if (selected)
                {
                    var tl = new TeamLabel() { TeamId = teamId, LabelId = labelId };
                    await cn.MergeAsync(tl, _data.CurrentUser);
                }
                else
                {
                    var tl = await cn.FindWhereAsync<TeamLabel>(new { TeamId = teamId, LabelId = labelId });
                    if (tl != null) await cn.DeleteAsync<TeamLabel>(tl.Id);
                }

                var labels = await new TeamLabelsInUseByTeam() { OrgId = _data.CurrentOrg.Id, TeamId = teamId }.ExecuteAsync(cn);
                return Content(labels.Count().ToString());
            }
        }

        public async Task<ContentResult> NewItemAppLabel(int applicationId, int labelId, bool selected)
        {
            using (var cn = _data.GetConnection())
            {
                if (selected)
                {
                    var nal = new NewItemAppLabel() { ApplicationId = applicationId, LabelId = labelId };
                    await cn.MergeAsync(nal, _data.CurrentUser);
                }
                else
                {
                    var nal = await cn.FindWhereAsync<NewItemAppLabel>(new { ApplicationId = applicationId, LabelId = labelId });
                    if (nal != null) await cn.DeleteAsync<NewItemAppLabel>(nal.Id);
                }

                var apps = await new NewItemAppLabelsInUse() { OrgId = _data.CurrentOrg.Id, LabelId = labelId }.ExecuteAsync(cn);
                return Content(apps.Count().ToString());
            }
        }

        [Route("/Update/CurrentTeam/{id}")]
        public async Task<RedirectResult> CurrentTeam(int id, string returnUrl)
        {
            if (_data.CurrentOrgUser == null) return Redirect(returnUrl);

            _data.CurrentOrgUser.CurrentTeamId = (id != 0) ? id : default(int?);
            _data.CurrentOrgUser.CurrentAppId = null;
            await _data.TryUpdateAsync(_data.CurrentOrgUser, r => r.CurrentTeamId, r => r.CurrentAppId);
            return Redirect(returnUrl);
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
                var myOrgs = await new MySwitchOrgs() { CurrentOrgId = _data.CurrentOrg.Id, UserId = _data.CurrentUser.UserId }.ExecuteAsync(cn);
                if (!myOrgs.Any(o => o.Id == id)) throw new Exception("You don't belong to this org.");
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

        public async Task<ContentResult> WorkItemTitle(string elementId, string newName)
        {
            int number = IntFromText(elementId);
            var workItem = await _data.FindWhereAsync<WorkItem>(new { OrganizationId = _data.CurrentOrg.Id, Number = number });
            workItem.Title = newName;
            await _data.TryUpdateAsync(workItem, r => r.Title);
            return Content(newName);
        }

        public async Task<ContentResult> ProjectName(string elementId, string newName)
        {
            int projectId = IntFromText(elementId);
            var project = await _data.FindAsync<Project>(projectId);
            if (project.Team.OrganizationId != _data.CurrentOrg.Id) throw new Exception("Application is in a different organization.");
            project.Name = newName;
            await _data.TryUpdateAsync(project, r => r.Name);
            return Content(newName);
        }

        public async Task<ContentResult> ProjectNickname(string elementId, string newName)
        {
            int projectId = IntFromText(elementId);
            var project = await _data.FindAsync<Project>(projectId);
            if (project.Application.OrganizationId != _data.CurrentOrg.Id) throw new Exception("Application is in a different organization.");
            project.Nickname = newName;
            await _data.TryUpdateAsync(project, r => r.Nickname);
            return Content(newName);
        }

        public async Task<ContentResult> MilestoneName(string elementId, string newName)
        {
            int milestoneId = IntFromText(elementId);
            var ms = await _data.FindAsync<Milestone>(milestoneId);
            if (ms.OrganizationId != _data.CurrentOrg.Id) throw new Exception("Application is in a different organization.");
            ms.Name = newName;
            await _data.TryUpdateAsync(ms, r => r.Name);
            return Content(newName);
        }

        private int IntFromText(string input)
        {
            var result = Regex.Match(input, @"\d+");
            return Convert.ToInt32(result.Value);
        }

        [HttpPost]
        public async Task<ContentResult> MilestoneDate()
        {
            string body = await Request.ReadStringAsync();
            var data = JsonConvert.DeserializeObject<MilestoneDateChange>(body);
            DateTime date = DateTime.Parse(data.DateText);

            using (var cn = _data.GetConnection())
            {
                var ms = await cn.FindAsync<Milestone>(data.MilestoneId);
                ms.Date = date;
                await cn.UpdateAsync(ms, _data.CurrentUser, r => r.Date);
            }

            return Content(date.ToString("ddd M/d"));
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
                { nameof(ActivitySubscription), async (cn) => await cn.FindAsync<ActivitySubscription>(id) },
                { nameof(OrganizationUser), async (cn) => await cn.FindAsync<OrganizationUser>(id) },
                { nameof(LabelSubscription), async (cn) => await cn.FindAsync<LabelSubscription>(id) }
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

        [HttpPost]
        public async Task<JsonResult> MilestoneDrop(int milestoneId, int year, int month)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var ms = await _data.FindAsync<Milestone>(cn, milestoneId);
                    DateTime newDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                    ms.Date = newDate;
                    await cn.UpdateAsync(ms, _data.CurrentUser, r => r.Date);
                    return Json(new { success = true, newDate = newDate.ToString("M/d") });
                }
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UserActivityOrder()
        {
            try
            {
                string body = await Request.ReadStringAsync();
                var data = JsonConvert.DeserializeObject<ProjectPriorityUpdate>(body);

                using (var cn = _data.GetConnection())
                {
                    await cn.ExecuteAsync("dbo.UpdateUserActivityOrder", new
                    {
                        userName = User.Identity.Name,
                        localTime = _data.CurrentUser.LocalTime,
                        orgId = _data.CurrentOrg.Id,
                        userId = _data.CurrentUser.UserId,
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
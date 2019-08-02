using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postulate.SqlServer.IntKey;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Controllers
{
    [Authorize]
    public class WorkItemController : Controller
    {
        private readonly DataAccess _data;
        private readonly IFreshdeskClientFactory _clientFactory;
        private readonly FreshdeskCache _freshdeskCache;

        public WorkItemController(
            IConfiguration config,
            IFreshdeskClientFactory freshdeskClientFactory)
        {
            _data = new DataAccess(config);
            _clientFactory = freshdeskClientFactory;
            _freshdeskCache = new FreshdeskCache(config, _clientFactory);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            _data.Initialize(User, TempData);
        }

        [HttpGet]
        public async Task<IActionResult> InfoBanner(int id)
        {
            if (id == 0)
            {
                return Content(string.Empty);
            }

            using (var cn = _data.GetConnection())
            {
                var workItem = await _data.FindWorkItemAsync(cn, id);
                return PartialView(workItem);
            }
        }

        [HttpPost]
        public async Task<RedirectResult> Create(WorkItem workItem, string returnUrl)
        {
            // make sure incoming number has not been set so that SetNumber method works
            workItem.Number = 0;

            // make sure item is part of this org
            workItem.OrganizationId = _data.CurrentOrg.Id;

            using (var cn = _data.GetConnection())
            {
                if (workItem.MilestoneId < 0)
                {
                    var indirectMilestones = new IndirectMilestones();
                    workItem.MilestoneId = await indirectMilestones.Options[workItem.MilestoneId.Value].GetMilestoneIdAsync(cn, _data.CurrentUser, workItem.OrganizationId, workItem.TeamId);
                }

                if (workItem.TeamId == 0)
                {
                    if (workItem.ApplicationId.HasValue)
                    {
                        var app = await cn.FindAsync<Application>(workItem.ApplicationId.Value);
                        workItem.TeamId = app.TeamId ?? _data.CurrentOrgUser.CurrentTeamId ?? 0;
                    }

                    if (workItem.TeamId == 0 && workItem.ProjectId.HasValue)
                    {
                        var prj = await cn.FindAsync<Project>(workItem.ProjectId.Value);
                        workItem.TeamId = prj.TeamId;
                        if (!workItem.ApplicationId.HasValue) workItem.ApplicationId = prj.ApplicationId;
                    }

                    if (workItem.TeamId == 0) throw new Exception("Couldn't determine the TeamId for the new work item.");
                }

                await workItem.SaveHtmlAsync(_data, cn);
                await workItem.SetNumberAsync(cn);
                if (await _data.TrySaveAsync(cn, workItem))
                {
                    if (workItem.ParseFreshdeskTicket(out int number))
                    {
                        // what if already linked?
                        var client = await _clientFactory.CreateClientForOrganizationAsync(_data.CurrentOrg.Id);
                        var ticket = await client.GetTicketAsync(number, true);
                        if (ticket != null)
                        {
                            await _freshdeskCache.LinkWorkItemToTicketAsync(cn, client, _data.CurrentOrg.Id, workItem.Number, ticket, _data.CurrentUser);
                        }                        
                    }

                    if (workItem.AssignToUserId.HasValue)
                    {
                        await AssignToInnerAsync(cn, workItem.AssignToUserId.Value, workItem);
                    }

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
                    var workItem = await _data.FindWorkItemAsync(cn, id);
                    var activity = await _data.FindAsync<Activity>(cn, activityId);
                    workItem.ActivityId = activityId;
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
        public async Task<JsonResult> ResumeActivity([FromForm]int id)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var workItem = await _data.FindWorkItemAsync(cn, id);
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

        public async Task<JsonResult> CancelActivity([FromForm]int id)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var workItem = await _data.FindWorkItemAsync(cn, id);
                    workItem.ActivityId = null;
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
                    var workItem = await _data.FindWorkItemAsync(cn, id);

                    if (workItem.ActivityId.HasValue)
                    {
                        var activity = await _data.FindAsync<Activity>(cn, workItem.ActivityId.Value);
                        Responsibility.ClearWorkItemUserActions[activity.ResponsibilityId].Invoke(workItem);
                    }
                    else
                    {
                        if (workItem?.DeveloperUserId.Equals(_data.CurrentUser.UserId) ?? false) workItem.DeveloperUserId = null;
                        if (workItem?.BusinessUserId.Equals(_data.CurrentUser.UserId) ?? false) workItem.BusinessUserId = null;
                    }

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
        public async Task<JsonResult> WorkOnNext([FromForm]int id)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var workItem = await _data.FindWorkItemAsync(cn, id);

                    // work item can have just one priority, so we look to see if there's one existing
                    if (!(await cn.ExistsWhereAsync<WorkItemPriority>(new { WorkItemId = workItem.Id })))
                    {
                        var nextPriority = await new NextPriority() { OrgId = _data.CurrentOrg.Id, TeamId = workItem.TeamId }.ExecuteSingleAsync(cn);
                        var wip = new WorkItemPriority()
                        {
                            WorkItemId = workItem.Id,
                            MilestoneId = 0,
                            UserId = 0,
                            Value = nextPriority.NextValue
                        };
                        if (await _data.TrySaveAsync(wip))
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, message = TempData[AlertCss.Error] });
                        }
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemovePriority([FromForm]int id)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var workItem = await _data.FindWorkItemAsync(cn, id);
                    var wip = await _data.FindWhereAsync<WorkItemPriority>(cn, new { WorkItemId = workItem.Id });
                    if (wip != null)
                    {
                        await _data.TryDeleteAsync<WorkItemPriority>(wip.Id);
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }

        public async Task<JsonResult> AssignTo(int id, int userId)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var workItem = await _data.FindWorkItemAsync(cn, id);
                    await AssignToInnerAsync(cn, userId, workItem);
                }
                return Json(new { success = true });
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }

        private async Task AssignToInnerAsync(SqlConnection cn, int userId, WorkItem workItem)
        {
            var orgUser = await _data.FindWhereAsync<OrganizationUser>(cn, new { OrganizationId = _data.CurrentOrg.Id, UserId = userId });

            workItem.DeveloperUserId = userId;
            workItem.ActivityId = orgUser.DefaultActivityId ?? _data.CurrentOrg.DeveloperActivityId.Value;

            if (await _data.TrySaveAsync(cn, workItem, new string[]
            {
                nameof(WorkItem.DeveloperUserId),
                nameof(WorkItem.ActivityId)
            }))
            {
                string body = $"{_data.CurrentOrgUser.GetDisplayName()} assigned {workItem.Number} to {orgUser.GetDisplayName()}";
                int eventLogId = await EventLog.WriteAsync(cn, new EventLog()
                {
                    EventId = SystemEvent.WorkItemAssigned,
                    IconClass = WorkItem.IconAssigned,
                    IconColor = "auto",
                    OrganizationId = workItem.OrganizationId,
                    TeamId = workItem.TeamId,
                    ApplicationId = workItem.ApplicationId,
                    WorkItemId = workItem.Id,
                    HtmlBody = body,
                    TextBody = body
                }, _data.CurrentUser);
                await Notification.CreateFromWorkItemAssignment(cn, eventLogId);
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

                    if (data.GroupFieldValue != 0 && !string.IsNullOrEmpty(data.GroupFieldName))
                    {                        
                        var customGrouping = new MyItemGroupingOption();
                        if (await DidGroupingChangeAsync(cn, workItem, customGrouping[data.GroupFieldName], data.GroupFieldValue))
                        {
                            customGrouping[data.GroupFieldName].UpdateWorkItem(cn, _data.CurrentUser, workItem, data.GroupFieldValue);
                        }                        
                    }

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

        /// <summary>
        /// We need to know if the grouping actually changed after a drag operation by looking at applicable work item field value
        /// </summary>
        private async Task<bool> DidGroupingChangeAsync(SqlConnection cn, WorkItem workItem, MyItemGroupingOption.Option option, int value)
        {
            // a regular WorkItem doesn't have everything that the "dto" version has (for custom grouping purposes), so I need to query that
            var item = await new OpenWorkItems() { IsOpen = null, OrgId = workItem.OrganizationId, Id = workItem.Id }.ExecuteSingleAsync(cn);
            return !option.GroupValueFunction(item).Equals(value);
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
            // can't use a zero as userId because the OrgUser FindWhere fails ("sequence contains more than one element")
            if (userId == 0) userId = -1;

            // get the responsibility from the work item activity (if set)
            // or fall back to the user profile
            int responsibilityId = (workItem.ActivityId.HasValue) ?
                (await cn.FindAsync<Activity>(workItem.ActivityId.Value)).ResponsibilityId :
                (await cn.FindWhereAsync<OrganizationUser>(new { OrganizationId = _data.CurrentOrg.Id, UserId = userId }) ?? new OrganizationUser()).Responsibilities;

            // if user has both biz and dev responsibility, assume dev
            if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;

            if (userId > 0)
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
                bool isPinned = await IsPinnedWorkItemAsync(cn, workItem.Id);
                if (milestoneId == -1)
                {
                    if (!isPinned) await PinWorkItemAsync(cn, workItem.Id);
                }
                else
                {                    
                    if (isPinned) await UnpinWorkItemAsync(cn, workItem.Id);
                    workItem.MilestoneId = (milestoneId == 0) ? default(int?) : milestoneId;
                    await cn.UpdateAsync(workItem, _data.CurrentUser, r => r.MilestoneId);
                }
            }
        }

        private async Task UnpinWorkItemAsync(SqlConnection cn, int id)
        {
            var pin = await cn.FindWhereAsync<PinnedWorkItem>(new { WorkItemId = id, _data.CurrentUser.UserId });
            if (pin != null) await cn.DeleteAsync<PinnedWorkItem>(pin.Id);
        }

        private async Task PinWorkItemAsync(SqlConnection cn, int id)
        {
            var pin = new PinnedWorkItem() { WorkItemId = id, UserId = _data.CurrentUser.UserId };
            await cn.SaveAsync(pin, _data.CurrentUser);
        }

        private async Task<bool> IsPinnedWorkItemAsync(SqlConnection cn, int id)
        {
            return await cn.ExistsWhereAsync<PinnedWorkItem>(new { WorkItemId = id, _data.CurrentUser.UserId });
        }

        [HttpPost]
        public async Task<PartialViewResult> SaveComment([FromForm]Comment comment)
        {
            using (var cn = _data.GetConnection())
            {
                comment.OrganizationId = _data.CurrentOrg.Id;
                await comment.SaveHtmlAsync(_data, cn);
                if (await _data.TrySaveAsync(comment)) await AddFreshdeskNoteAsync(comment, cn);

                var vm = new CommentView();
                vm.ObjectId = comment.ObjectId;
                vm.ObjectType = comment.ObjectType;
                vm.Comments = await new Comments() { OrgId = _data.CurrentOrg.Id, ObjectType = comment.ObjectType, ObjectIds = new int[] { comment.ObjectId } }.ExecuteAsync(cn);
                return PartialView("/Pages/Dashboard/Items/_Comments.cshtml", vm);
            }
        }

        private async Task AddFreshdeskNoteAsync(Comment comment, SqlConnection cn)
        {
            if (comment.ObjectType == ObjectType.WorkItem)
            {
                var workItem = await cn.FindAsync<WorkItem>(comment.ObjectId);
                if (!workItem.Organization.UseFreshdesk()) return;

                if ((workItem.WorkItemTicket?.WorkItemNumber ?? 0) != 0)
                {
                    var client = await _clientFactory.CreateClientForOrganizationAsync(workItem.OrganizationId);
                    await client.AddNoteAsync(workItem.WorkItemTicket.TicketId, comment, _data.UserDisplayName);
                }
            }
        }

        public async Task<JsonResult> GetApps(int teamId)
        {
            using (var cn = _data.GetConnection())
            {
                var results = await new AppSelect() { OrgId = _data.CurrentOrg.Id, TeamId = teamId }.ExecuteAsync(cn);
                return Json(results);
            }
        }

        public async Task<JsonResult> GetAppProjects(int appId)
        {
            using (var cn = _data.GetConnection())
            {                
                var results = await new ProjectSelect() { AppId = appId }.ExecuteAsync(cn);

                var app = await cn.FindAsync<Application>(appId);
                var globalProjects = await new ProjectSelect() { TeamId = app.TeamId, AppId = 0 }.ExecuteAsync(cn);

                return Json(results.Concat(globalProjects).OrderBy(item => item.Text));
            }
        }

        public async Task<JsonResult> GetTeamProjects(int teamId, int? appId = null)
        {
            using (var cn = _data.GetConnection())
            {
                var results = await new ProjectSelect() { TeamId = teamId, AppId = appId }.ExecuteAsync(cn);
                return Json(results);
            }
        }

        public async Task<JsonResult> GetAppMilestones(int appId)
        {
            using (var cn = _data.GetConnection())
            {
                var results = await new MilestoneSelect() { OrgId = appId }.ExecuteAsync(cn);
                return Json(results);
            }
        }

        [HttpPost]
        public async Task<JsonResult> SetMilestone()
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var json = await Request.ReadStringAsync();
                    var data = JsonConvert.DeserializeAnonymousType(json, new
                    {
                        number = 0,
                        milestoneDate = DateTime.MinValue
                    });

                    var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = data.number });
                    var ms = (data.milestoneDate != DateTime.MaxValue) ?
                        await _data.FindWhereAsync<Milestone>(cn, new { OrganizationId = _data.CurrentOrg.Id, Date = data.milestoneDate }) :
                        default(Milestone);

                    workItem.MilestoneId = ms?.Id;
                    await cn.UpdateAsync(workItem, _data.CurrentUser, r => r.MilestoneId);
                }
                return Json(new { success = true });
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }

        [HttpGet]
        public async Task<PartialViewResult> Attachments(int id)
        {
            throw new NotImplementedException();
        }

        public IActionResult View(int id) => RedirectToPage("/WorkItem/View", new { id });

        private async Task<WorkItemListView> ListInnerAsync(OpenWorkItems query)
        {
            var vm = new WorkItemListView();

            using (var cn = _data.GetConnection())
            {
                vm.WorkItems = await query.ExecuteAsync(cn);
                var itemIds = vm.WorkItems.Select(wi => wi.Id).ToArray();
                var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = _data.CurrentOrg.Id }.ExecuteAsync(cn);
                vm.SelectedLabels = labelsInUse.ToLookup(row => row.WorkItemId);
            }

            return vm;
        }

        public async Task<PartialViewResult> ListInMilestone(int id)
        {
            var vm = await ListInnerAsync(new OpenWorkItems()
            {
                OrgId = _data.CurrentOrg.Id,
                MilestoneId = id
            });

            return PartialView("List", vm);
        }

        public async Task<PartialViewResult> ListInProject(int id, int? year, int? month)
        {
            var vm = await ListInnerAsync(new OpenWorkItems()
            {
                OrgId = _data.CurrentOrg.Id,
                ProjectId = id,
                MilestoneYear = year,
                MilestoneMonth = month
            });

            return PartialView("List", vm);
        }

        public async Task<JsonResult> Close(int id, int reasonId)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var wi = await _data.FindWorkItemAsync(cn, id);
                    wi.CloseReasonId = reasonId;
                    wi.ModifiedBy = User.Identity.Name;
                    wi.DateModified = _data.CurrentUser.LocalTime;
                    await cn.UpdateAsync(wi, _data.CurrentUser, r => r.CloseReasonId, r => r.ModifiedBy, r => r.DateModified);
                }
                return Json(new { success = true });
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }

        public async Task<PartialViewResult> SetProject(int id, int projectId)
        {
            ProjectInfoResult prj = null;

            using (var cn = _data.GetConnection())
            {
                var wi = await _data.FindWorkItemAsync(cn, id);
                wi.ProjectId = projectId;
                wi.ModifiedBy = User.Identity.Name;
                wi.DateModified = _data.CurrentUser.LocalTime;
                await cn.UpdateAsync(wi, _data.CurrentUser, r => r.ProjectId, r => r.ModifiedBy, r => r.DateModified);

                prj = await new ProjectInfo() { OrgId = _data.CurrentOrg.Id, Id = projectId }.ExecuteSingleAsync(cn);
            }

            return PartialView("/Pages/Dashboard/Items/_ItemInfo.cshtml", prj);
        }

        public async Task<JsonResult> TogglePin(int id)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var wi = await _data.FindWorkItemAsync(cn, id);
                    var pin =
                        await _data.FindWhereAsync<PinnedWorkItem>(new { _data.CurrentUser.UserId, WorkItemId = wi.Id }) ??
                        new PinnedWorkItem() { UserId = _data.CurrentUser.UserId, WorkItemId = wi.Id };

                    string addClass = null;
                    string removeClass = null;
                    if (pin.Id == 0)
                    {
                        addClass = PinnedWorkItem.PinnedIcon;
                        removeClass = PinnedWorkItem.UnpinnedIcon;
                        await cn.SaveAsync(pin, _data.CurrentUser);
                    }
                    else
                    {
                        addClass = PinnedWorkItem.UnpinnedIcon;
                        removeClass = PinnedWorkItem.PinnedIcon;
                        await cn.DeleteAsync<PinnedWorkItem>(pin.Id);
                    }
                    return Json(new { success = true, addClass, removeClass });
                }                
            }
            catch (Exception exc)
            {
                return Json(new { success = false, message = exc.Message });
            }
        }
    }
}
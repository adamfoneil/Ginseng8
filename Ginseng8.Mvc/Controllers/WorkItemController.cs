using Dapper;
using Ginseng.Integration.Services;
using Ginseng.Models;
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

        public WorkItemController(
            IConfiguration config,
            IFreshdeskClientFactory freshdeskClientFactory)
        {
            _data = new DataAccess(config);
            _clientFactory = freshdeskClientFactory;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            _data.Initialize(User, TempData);
        }

        [HttpGet]
        public async Task<PartialViewResult> InfoBanner(int id)
        {
            using (var cn = _data.GetConnection())
            {
                var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
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
                await workItem.SaveHtmlAsync(_data, cn);
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

        public async Task<JsonResult> CancelActivity([FromForm]int id)
        {
            try
            {
                using (var cn = _data.GetConnection())
                {
                    var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
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
                    var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });

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
                    var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });

                    // work item can have just one priority, so we look to see if there's one existing
                    if (!(await cn.ExistsWhereAsync<WorkItemPriority>(new { WorkItemId = workItem.Id })))
                    {
                        var nextPriority = await new NextPriority() { OrgId = _data.CurrentOrg.Id, AppId = workItem.ApplicationId }.ExecuteSingleAsync(cn);
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
                    var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
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
                    var workItem = await _data.FindWhereAsync<WorkItem>(cn, new { OrganizationId = _data.CurrentOrg.Id, Number = id });
                    var orgUser = await _data.FindWhereAsync<OrganizationUser>(cn, new { OrganizationId = _data.CurrentOrg.Id, UserId = userId });

                    workItem.DeveloperUserId = userId;
                    workItem.ActivityId = orgUser.DefaultActivityId ?? _data.CurrentOrg.DeveloperActivityId.Value;

                    if (await _data.TrySaveAsync(cn, workItem, new string[] 
                    {
                        nameof(WorkItem.DeveloperUserId),
                        nameof(WorkItem.ActivityId)
                    }))
                    {
                        // send email
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
                workItem.MilestoneId = (milestoneId == 0) ? default(int?) : milestoneId;
                await cn.UpdateAsync(workItem, _data.CurrentUser, r => r.MilestoneId);
            }
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

        public async Task<JsonResult> GetAppProjects(int appId)
        {
            using (var cn = _data.GetConnection())
            {
                var results = await new ProjectSelect() { AppId = appId }.ExecuteAsync(cn);
                return Json(results);
            }
        }

        public async Task<JsonResult> GetAppMilestones(int appId)
        {
            using (var cn = _data.GetConnection())
            {
                var results = await new MilestoneSelect() { AppId = appId }.ExecuteAsync(cn);
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

        public async Task<PartialViewResult> ListInProject(int id)
        {
            var vm = await ListInnerAsync(new OpenWorkItems()
            {
                OrgId = _data.CurrentOrg.Id,
                ProjectId = id
            });            

            return PartialView("List", vm);
        }
    }
}
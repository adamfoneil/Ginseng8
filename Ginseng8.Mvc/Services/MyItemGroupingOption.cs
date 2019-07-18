using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Ginseng.Mvc.Services
{
    /// <summary>
    /// what do we show after the WI number in card titles?
    /// </summary>
    [Flags]
    public enum WorkItemTitleViewField
    {
        Project = 1,
        Application = 2
    }    

    /// <summary>
    /// What do we pass as the milestoneId to our proc that update WI prioerties?
    /// Normally this is the milestoneId itself. However when grouping by work day, we treat
    /// the work day index as the milestoneId instead
    /// </summary>
    public enum MilestoneArgumentSource
    {
        MilestoneId,
        GroupValue
    }

    /// <summary>
    /// Provides functionality for changing the grouping of work items on the Team and MyItems pages
    /// between project-only (for Becky) and the team options (which uses the Team.UseApplications
    /// property to switch between app and project grouping)
    /// </summary>
    public class MyItemGroupingOption
    {
        public const string ProjectParentId = "ProjectParentId";
        public const string ProjectId = "ProjectId";
        public const string ActivityId = "ActivityId";
        public const string WorkDay = "WorkDay";

        public IEnumerable<Option> GetOptions()
        {
            yield return new Option()
            {
                Value = ProjectParentId,
                Text = "Use Team Settings",
                GroupValueFunction = (item) => item.ProjectParentId,
                GroupSortFunction = (item) => item.ProjectParentName,
                GroupHeadingFunction = (item) => item.ProjectParentName,
                FieldNameFunction = (item) => item.ProjectParentField,
                TitleViewField = WorkItemTitleViewField.Project,
                WorkItemQuerySort = OpenWorkItemsSortOptions.Priority,
                UpdateWorkItem = (cn, user, wi, value) =>
                {
                    if (wi.Team.UseApplications)
                    {
                        wi.ApplicationId = value;
                        cn.Update(wi, user, r => r.ApplicationId);
                    }
                    else
                    {
                        wi.TeamId = value;
                        cn.Update(wi, user, r => r.TeamId);
                    }
                }
            };

            yield return new Option()
            {
                Value = ProjectId,
                Text = "Project",
                GroupValueFunction = (item) => item.ProjectId,
                GroupHeadingFunction = (item) => item.ProjectName,
                GroupSortFunction = (item) => item.ProjectName,
                FieldNameFunction = (item) => nameof(item.ProjectId),
                TitleViewField = WorkItemTitleViewField.Application,
                WorkItemQuerySort = OpenWorkItemsSortOptions.Priority,
                UpdateWorkItem = (cn, user, wi, value) =>
                {
                    wi.ProjectId = value;
                    cn.Update(wi, user, r => r.ProjectId);
                }
            };

            yield return new Option()
            {
                Value = ActivityId,
                Text = "Activity",
                GroupValueFunction = (item) => item.ActivityId,
                GroupHeadingFunction = (item) => item.ActivityName,
                GroupSortFunction = (item) => item.MyActivityOrder ?? 0,
                FieldNameFunction = (item) => nameof(item.ActivityId),
                TitleViewField = WorkItemTitleViewField.Project | WorkItemTitleViewField.Application,
                WorkItemQuerySort = OpenWorkItemsSortOptions.Priority,
                UpdateWorkItem = (cn, user, wi, value) =>
                {
                    wi.ActivityId = value;
                    var act = cn.Find<Activity>(value);
                    switch (act.ResponsibilityId)
                    {
                        case 1: // biz
                            if (!wi.BusinessUserId.HasValue) wi.BusinessUserId = wi.DeveloperUserId;
                            break;

                        case 2: // dev
                            if (!wi.DeveloperUserId.HasValue) wi.DeveloperUserId = wi.BusinessUserId;
                            break;
                    }
                    cn.Update(wi, user, r => r.ActivityId, r => r.DeveloperUserId, r => r.BusinessUserId);
                }
            };

            yield return new Option()
            {
                Value = WorkDay,
                Text = "Work Day",
                GroupValueFunction = (item) => item.WorkDay,
                GroupSortFunction = (item) => item.WorkDay,
                GroupHeadingFunction = (item) => (item.WorkDay == OpenWorkItems.UnscheduledWorkDay) ? "Unscheduled" : $"{item.WorkDay}: {item.WorkDayDate.ToString("ddd M/d")}",
                FieldNameFunction = (item) => nameof(item.WorkDay),
                PriorityUpdateProcedure = "dbo.UpdateWorkItemUserPriorities",
                ChangeDetectIgnoreValue = -1,
                UpdateWorkItem = (cn, user, wi, value) =>
                {
                    var userProfile = cn.FindWhere<UserProfile>(new { user.UserName });
                    var wiup =
                        cn.FindWhere<WorkItemUserPriority>(new { WorkItemId = wi.Id, userProfile.UserId }) ??
                        new WorkItemUserPriority() { WorkItemId = wi.Id, UserId = userProfile.UserId };

                    if (value != OpenWorkItems.UnscheduledWorkDay)
                    {
                        wiup.Value = value;
                        cn.Save(wiup, user);
                    }
                    else
                    {
                        // deleting the work item user priority is how you send something to your "Unscheduled" group
                        if (wiup.Id != 0) cn.Delete<WorkItemUserPriority>(wiup.Id);
                    }                    
                },
                WorkItemQuerySort = OpenWorkItemsSortOptions.WorkDay,
                TitleViewField = WorkItemTitleViewField.Application | WorkItemTitleViewField.Project,
                MilestoneArgumentSource = MilestoneArgumentSource.GroupValue
            };
        }

        public SelectList GetSelectList(string currentValue)
        {
            var items = GetOptions().Select(opt => new SelectListItem() { Value = opt.Value, Text = opt.Text });
            return new SelectList(items, "Value", "Text", currentValue);
        }

        public Option this[string name]
        {
            get
            {
                var dictionary = GetOptions().ToDictionary(item => item.Value);
                // this is a hack to deal with the fact that the group field name can be a variable not reflected in the actual dictionary keys,
                // so I just assume "ProjectParentId" if the key is not present
                return (dictionary.ContainsKey(name)) ? dictionary[name] : dictionary[ProjectParentId];
            }
        }

        public class Option
        {
            /// <summary>
            /// Stored option value
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Text displayed in option dropdown
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Function that returns grouping value
            /// </summary>
            public Func<OpenWorkItemsResult, int> GroupValueFunction { get; set; }

            /// <summary>
            /// Function that returns the grouping sort order
            /// </summary>
            public Func<OpenWorkItemsResult, IComparable> GroupSortFunction { get; set; }

            /// <summary>
            /// Function that returns the group heading
            /// </summary>
            public Func<OpenWorkItemsResult, string> GroupHeadingFunction { get; set; }

            /// <summary>
            /// Function that returns the field name to use in the InsertItem form
            /// </summary>
            public Func<OpenWorkItemsResult, string> FieldNameFunction { get; set; }

            /// <summary>
            /// Action that updates a field in the work item it's dragged on the My Items or Teams page.
            /// This should set a field that correspnds to the Value property here
            /// </summary>
            public Action<IDbConnection, IUser, WorkItem, int> UpdateWorkItem { get; set; }

            public WorkItemTitleViewField TitleViewField { get; set; }

            public OpenWorkItemsSortOptions WorkItemQuerySort { get; set; }

            /// <summary>
            /// What procedure do we run to capture the new work item priorities?
            /// By default we update biz priorities with dbo.UpdateWorkItemPriorities,
            /// but when grouping by work day, we use dbo.UpdateWorkItemUserPriorities
            /// </summary>
            public string PriorityUpdateProcedure { get; set; } = "dbo.UpdateWorkItemPriorities";

            /// <summary>
            /// What value in the group field do we ignore as a change?
            /// Normally this is left at 0, but WorkDay grouping uses -1 because 0 is a valid value for WorkDay
            /// </summary>
            public int ChangeDetectIgnoreValue { get; set; }

            public MilestoneArgumentSource MilestoneArgumentSource { get; set; } = MilestoneArgumentSource.MilestoneId;
        }
    }
}
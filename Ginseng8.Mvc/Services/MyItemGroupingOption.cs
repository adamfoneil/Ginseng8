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
    /// Provides functionality for changing the grouping of work items on the Team and MyItems pages
    /// between project-only (for Becky) and the team options (which uses the Team.UseApplications
    /// property to switch between app and project grouping)
    /// </summary>
    public class MyItemGroupingOption
    {
        public const string ProjectParentId = "ProjectParentId";
        public const string ProjectId = "ProjectId";
        public const string ActivityId = "ActivityId";

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
        }
    }
}
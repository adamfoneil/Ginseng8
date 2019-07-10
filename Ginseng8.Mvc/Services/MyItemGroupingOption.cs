using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Services
{
    /// <summary>
    /// what do we show after the WI number in card titles?
    /// </summary>
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
        public IEnumerable<Option> GetOptions()
        {
            yield return new Option()
            {
                Value = "ProjectParentId",
                Text = "Use Team Settings",
                GroupValueFunction = (item) => item.ProjectParentId,
                GroupHeadingFunction = (item) => item.ProjectParentName,
                FieldNameFunction = (item) => item.ProjectParentField,
                TitleViewField = WorkItemTitleViewField.Project
            };

            yield return new Option()
            {
                Value = "ProjectId",
                Text = "Project",
                GroupValueFunction = (item) => item.ProjectId,
                GroupHeadingFunction = (item) => item.ProjectName,
                FieldNameFunction = (item) => nameof(item.ProjectId),
                TitleViewField = WorkItemTitleViewField.Application
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
                return dictionary[name];
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
            /// Function that returns the group heading
            /// </summary>
            public Func<OpenWorkItemsResult, string> GroupHeadingFunction { get; set; }

            /// <summary>
            /// Function that returns the field name to use in the InsertItem form
            /// </summary>
            public Func<OpenWorkItemsResult, string> FieldNameFunction { get; set; }

            public WorkItemTitleViewField TitleViewField { get; set; }
        }
    }
}
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Services
{
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
                GroupFunction = (item) => item.ProjectParentId,
                NameFunction = (item) => item.ProjectParentName,
                NewItemFieldName = (item) => item.ProjectParentField
            };

            yield return new Option()
            {
                Value = "ProjectId",
                Text = "Project",
                GroupFunction = (item) => item.ProjectId,
                NameFunction = (item) => item.ProjectName,
                NewItemFieldName = (item) => "ProjectId"
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

            public Func<OpenWorkItemsResult, int> GroupFunction { get; set; }
            public Func<OpenWorkItemsResult, string> NameFunction { get; set; }
            public Func<OpenWorkItemsResult, string> NewItemFieldName { get; set; }
        }
    }
}
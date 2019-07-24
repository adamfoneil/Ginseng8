using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Services
{
    public class MyItemUserIdFieldOption
    {
        public IEnumerable<Option> GetOptions()
        {
            yield return new Option()
            {
                Value = "AssignedUserId",
                Text = "Assigned User",
                Criteria = (qry, value) => qry.AssignedUserId = value
            };

            yield return new Option()
            {
                Value = "DeveloperUserId",
                Text = "Developer User",
                Criteria = (qry, value) => qry.DeveloperUserId = value
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
            public string Value { get; set; }
            public string Text { get; set; }
            public Action<OpenWorkItems, int> Criteria { get; set; }
        }
    }
}

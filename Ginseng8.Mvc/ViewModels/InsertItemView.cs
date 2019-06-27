using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ginseng.Mvc.ViewModels
{
	public class InsertItemView
	{
        // this is so the insert work item fields can have a unique auto-increment number that will never conflict with existing work items
        private static int _dummyNumber = 0; 

        public InsertItemView(Dictionary<string, int> contextValues)
		{			
			ContextValues = contextValues;
		}

        public CommonDropdowns Dropdowns { get; set; }
        public bool UseApplications { get; set; }
        public IEnumerable<SelectListItem> AssignToUsers { get; set; }

        public Dictionary<string, int> ContextValues { get; }

        public string Context
		{
			get { return string.Join("-", ContextValues.Select(kp => $"{kp.Key}-{kp.Value}")); }
		}

        public OpenWorkItemsResult GetDefaultWorkItem()
        {
            OpenWorkItemsResult result = new OpenWorkItemsResult();

            _dummyNumber--;
            result.Number = _dummyNumber;
            result.UseApplications = UseApplications;
            
            var type = result.GetType();
            var properties = type.GetProperties();
            foreach (var kp in ContextValues.Where(kp => kp.Value != 0))
            {
                if (FindProperty(properties, kp.Key, out PropertyInfo property)) property.SetValue(result, kp.Value);
            }

            return result;
        }

        private bool FindProperty(IEnumerable<PropertyInfo> properties, string propertyName, out PropertyInfo result)
        {
            try
            {                
                result = properties.Single(p => p.Name.ToLower().Equals(propertyName.ToLower()));
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public IHtmlContent WriteContextFields(IHtmlHelper html)
		{
            // filter out zeroes from fields because they are FK violations
            // also filter out fields that are rendered in dropdowns

            // if we're using Dropdowns, then it  means we need to exclude the visible fields from the hidden fields
            var visibleFields = (Dropdowns != null) ?
                new [] { "teamId", "applicationId", "projectId", "milestoneId", "sizeId" }.Select(s => s.ToLower()) :
                Enumerable.Empty<string>();

			foreach (var kp in ContextValues.Where(kp => kp.Value != 0 && !visibleFields.Contains(kp.Key.ToLower())))
			{
				TagBuilder input = new TagBuilder("input");
				input.MergeAttribute("type", "hidden");
				input.MergeAttribute("name", kp.Key);
				input.MergeAttribute("value", kp.Value.ToString());
				html.ViewContext.Writer.Write(input);				
			}

			var returnUrl = new TagBuilder("input");
			returnUrl.MergeAttribute("type", "hidden");
			returnUrl.MergeAttribute("name", "returnUrl");
			returnUrl.MergeAttribute("value", UriHelper.GetDisplayUrl(html.ViewContext.HttpContext.Request));
			html.ViewContext.Writer.Write(returnUrl);

			return null;
		}
	}
}
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
                PropertyInfo property = FindProperty(properties, kp.Key);
                property.SetValue(result, kp.Value);
            }

            return result;
        }

        private PropertyInfo FindProperty(IEnumerable<PropertyInfo> properties, string propertyName)
        {
            try
            {                
                return properties.Single(p => p.Name.ToLower().Equals(propertyName.ToLower()));
            }
            catch (Exception exc)
            {
                throw new Exception($"Couldn't find property {propertyName}", exc);
            }
        }

        public IHtmlContent WriteContextFields(IHtmlHelper html)
		{
            // filter out zeroes from fields because they are FK violations
            // also filter out fields that are rendered in dropdowns

            // didn't know another way to identify these
            var visibleFields = new [] { "teamId", "applicationId", "projectId", "milestoneId", "sizeId" }.Select(s => s.ToLower());

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
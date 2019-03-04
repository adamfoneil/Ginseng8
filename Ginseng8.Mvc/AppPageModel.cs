using Ginseng.Models;
using Ginseng.Models.Conventions;
using Ginseng.Mvc.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Postulate.Base.Exceptions;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	public class AppPageModel : PageModel
	{
		public AppPageModel(IConfiguration config)
		{	
			Data = new DataAccess(config);
		}

		public string OrgName => Data.CurrentOrg?.Name ?? "(no org)";

		protected DataAccess Data { get; }
		protected Organization CurrentOrg { get { return Data.CurrentOrg; } }
		protected UserProfile CurrentUser { get { return Data.CurrentUser; } }
		protected OrganizationUser CurrentOrgUser { get { return Data.CurrentOrgUser; } }
		
		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			base.OnPageHandlerExecuting(context);			
			Data.Initialize(User, TempData);			

			if (IsOrgRequired(context) && !CurrentUser.OrganizationId.HasValue)
			{
				context.Result = new RedirectResult("/Setup/Organization?mustCreate=true");
			}
		}

		private bool IsOrgRequired(PageHandlerExecutingContext context)
		{			
			var attr = context.HandlerMethod.MethodInfo.GetCustomAttributes(typeof(OrgNotRequired), false);
			if (attr.Any()) return false;

			attr = context.HandlerInstance.GetType().GetCustomAttributes(typeof(OrgNotRequired), false);
			if (attr.Any()) return false;

			return true;
		}
	}
}
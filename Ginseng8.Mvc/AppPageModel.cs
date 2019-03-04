using Ginseng.Models;
using Ginseng.Mvc.Attributes;
using Ginseng.Mvc.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Ginseng.Mvc
{
	public class AppPageModel : PageModel, IUserInfo
	{
		public AppPageModel(IConfiguration config)
		{
			Data = new DataAccess(config);
		}

		public string OrgName => Data.CurrentOrg?.Name ?? "(no org)";

		protected DataAccess Data { get; }
		public Organization CurrentOrg { get { return Data.CurrentOrg; } }
		public UserProfile CurrentUser { get { return Data.CurrentUser; } }
		public OrganizationUser CurrentOrgUser { get { return Data.CurrentOrgUser; } }
		public int UserId { get { return CurrentUser?.UserId ?? 0; } }
		public int OrgId { get { return CurrentUser?.OrganizationId ?? 0; } }
		public DateTime LocalTime { get { return CurrentUser.LocalTime; } }

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			base.OnPageHandlerExecuting(context);
			Data.Initialize(User, TempData);

			if (IsOrgRequired(context) && !CurrentUser.OrganizationId.HasValue)
			{
				context.Result = new RedirectResult("/Setup/Organization?mustCreate=true");
				return;
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
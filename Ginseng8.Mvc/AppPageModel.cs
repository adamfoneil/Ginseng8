using Ginseng.Models;
using Ginseng.Models.Conventions;
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
			Data = new DataAccess(User, config);
		}

		public string OrgName => Data.CurrentOrg?.Name ?? "(no org)";

		protected DataAccess Data { get; }
		protected Organization CurrentOrg { get { return Data.CurrentOrg; } }
		protected UserProfile CurrentUser { get { return Data.CurrentUser; } }
		protected OrganizationUser CurrentOrgUser { get { return Data.CurrentOrgUser; } }

		public ActionMessage ActionMessage { get { return Data.ActionMessage; } }

		[TempData]
		public string ActionMessageContent
		{
			get { return Data.ActionMessage.Content; }
			set { Data.ActionMessage.Content = value; }
		}

		[TempData]
		public ActionMessageType ActionMessageType
		{
			get { return Data.ActionMessage.Type; }
			set { Data.ActionMessage.Type = value; }
		}
		
		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			base.OnPageHandlerExecuting(context);
			Data.GetCurrentUser();
		}
	}
}
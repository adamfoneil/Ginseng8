using System.Data.SqlClient;
using System.Threading.Tasks;
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Dashboard
{
	[Authorize]
	public class MyItemsModel : DashboardPageModel
	{
		public MyItemsModel(IConfiguration config) : base(config)
		{			
		}

		/// <summary>
		/// What column is user Id assigned to when items are created?
		/// </summary>
		public string UserIdColumnName { get; set; }				

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				AssignedUserId = UserId,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};
		}

		protected override void OnGetInternal(SqlConnection connection)
		{
			int responsibilityId = CurrentOrgUser.Responsibilities;
			// if you have dev and biz responsibility, then assume dev
			if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;
			UserIdColumnName = Responsibility.WorkItemColumnName[responsibilityId];
		}
	}
}
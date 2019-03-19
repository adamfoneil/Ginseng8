using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
	public class TeamModel : DashboardPageModel
	{
		public TeamModel(IConfiguration config) : base(config)
		{
		}

		public ILookup<int, ClosedWorkItemsResult> ClosedItems { get; set; }

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			var closedItems = await new ClosedWorkItems() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
			ClosedItems = closedItems.ToLookup(row => row.DeveloperUserId ?? 0);
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};
		}
	}
}
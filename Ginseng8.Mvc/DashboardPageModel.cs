using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	[Authorize]
	public abstract class DashboardPageModel : AppPageModel
	{
		public DashboardPageModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<AllWorkItemsResult> WorkItems { get; set; }
		public ILookup<int, Label> SelectedLabels { get; set; }
		public CommonDropdowns Dropdowns { get; set; }

		/// <summary>
		/// Implement this to get the query for the dashboard
		/// </summary>
		protected abstract AllWorkItems GetQuery();

		/// <summary>
		/// Override this to populate additional model properties during the OnGetAsync method
		/// </summary>
		protected virtual async Task OnGetInternalAsync(SqlConnection connection)
		{
			await Task.CompletedTask;
		}

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				WorkItems = await GetQuery().ExecuteAsync(cn);

				int[] itemIds = WorkItems.Select(wi => wi.Id).ToArray();
				var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = OrgId }.ExecuteAsync(cn);
				SelectedLabels = labelsInUse.ToLookup(row => row.WorkItemId);

				Dropdowns = await CommonDropdowns.FillAsync(cn, OrgId, CurrentOrgUser.Responsibilities);
				await OnGetInternalAsync(cn);
			}
		}
	}
}
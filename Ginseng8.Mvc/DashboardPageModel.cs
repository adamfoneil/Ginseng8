using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
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

		public IEnumerable<OpenWorkItemsResult> WorkItems { get; set; }		
		public ILookup<int, Label> SelectedLabels { get; set; }		
		public IEnumerable<IGrouping<int, Label>> LabelFilter { get; set; }
		public CommonDropdowns Dropdowns { get; set; }
	
		[BindProperty(SupportsGet = true)]
		public int? LabelId { get; set; }

		/// <summary>
		/// Implement this to get the query for the dashboard
		/// </summary>
		protected abstract OpenWorkItems GetQuery();

		/// <summary>
		/// Override this to populate additional model properties during the OnGetAsync method
		/// </summary>
		protected virtual async Task OnGetInternalAsync(SqlConnection connection)
		{
			await Task.CompletedTask;
		}

		/// <summary>
		/// Override this to populate individual model properties that won't benefit from async execution
		/// </summary>		
		protected virtual void OnGetInternal(SqlConnection connection)
		{
			// do nothing by default
		}

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				var query = GetQuery();
				if (query != null)
				{
					WorkItems = await GetQuery().ExecuteAsync(cn);

					int[] itemIds = WorkItems.Select(wi => wi.Id).ToArray();
					var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = OrgId }.ExecuteAsync(cn);
					SelectedLabels = labelsInUse.ToLookup(row => row.WorkItemId);
					LabelFilter = labelsInUse.GroupBy(row => row.Id);
				}

				Dropdowns = await CommonDropdowns.FillAsync(cn, OrgId, CurrentOrgUser.Responsibilities);
			
				await OnGetInternalAsync(cn);

				OnGetInternal(cn);
			}
		}

		private int UpTo(int value, int max)
		{
			if (value > max) return max;
			return value;
		}
	}
}
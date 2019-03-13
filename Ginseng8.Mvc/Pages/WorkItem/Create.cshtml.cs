using Ginseng.Models;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class CreateModel : AppPageModel
	{
		public CreateModel(IConfiguration config) : base(config)
		{
		}

		public AllWorkItemsResult WorkItemResult { get; private set; }
		public IEnumerable<Label> SelectedLabels { get; set; }
		public CommonDropdowns Dropdowns { get; private set; }

		public async Task OnPostAsync(Models.WorkItem workItem)
		{
			// make sure incoming number has not been set so that SetNumber method works
			workItem.Number = 0;

			// make sure item is part of this org
			workItem.OrganizationId = Data.CurrentOrg.Id;

			workItem.SaveHtml();

			using (var cn = Data.GetConnection())
			{
				await workItem.SetNumberAsync(cn);
				await Data.TrySaveAsync(cn, workItem);
				Dropdowns = await CommonDropdowns.FillAsync(cn, OrgId, CurrentOrgUser.Responsibilities);
				WorkItemResult = await new AllWorkItems() { OrgId = OrgId, Number = workItem.Number }.ExecuteSingleAsync(cn);				
				SelectedLabels = await new LabelsInUse() { WorkItemIds = new[] { workItem.Id }, OrgId = OrgId }.ExecuteAsync(cn);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.WorkItem
{
	/// <summary>
	/// thanks to https://www.mikesdotnetting.com/article/325/partials-and-ajax-in-razor-pages
	/// </summary>
	public class ListModel : AppPageModel
    {
		public ListModel(IConfiguration config) : base(config)
		{

		}		

		public async Task<IActionResult> OnGetAsync(int projectId)
        {
			var vm = new WorkItemListView();

			using (var cn = Data.GetConnection())
			{ 				
				vm.WorkItems = await new OpenWorkItems()
				{
					OrgId = Data.CurrentOrg.Id,
					ProjectId = projectId
				}.ExecuteAsync(cn);

				var itemIds = vm.WorkItems.Select(wi => wi.Id).ToArray();
				var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = Data.CurrentOrg.Id }.ExecuteAsync(cn);
				vm.SelectedLabels = labelsInUse.ToLookup(row => row.WorkItemId);
			}

			return new PartialViewResult()
			{
				ViewName = "List",
				ViewData = new ViewDataDictionary<WorkItemListView>(ViewData, vm)
			};
		}
	}
}
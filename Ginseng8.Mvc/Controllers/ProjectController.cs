using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Controllers
{
	[Authorize]
	public class ProjectController : Controller
	{
		private readonly DataAccess _data;

		public ProjectController(IConfiguration config)
		{
			_data = new DataAccess(config);
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);
			_data.Initialize(User, TempData);
		}

		public async Task<PartialViewResult> WorkItems(int id)
		{
			var vm = new WorkItemListView();

			using (var cn = _data.GetConnection())
			{
				vm.WorkItems = await new OpenWorkItems()
				{
					OrgId = _data.CurrentOrg.Id,
					ProjectId = id
				}.ExecuteAsync(cn);

				var itemIds = vm.WorkItems.Select(wi => wi.Id).ToArray();
				var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = _data.CurrentOrg.Id }.ExecuteAsync(cn);
				vm.SelectedLabels = labelsInUse.ToLookup(row => row.WorkItemId);
			}

			return PartialView(vm);
		}
	}
}
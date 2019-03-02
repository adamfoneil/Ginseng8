using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.ViewComponents
{
	public class NewItemViewComponent : ViewComponent
	{
		private readonly DataAccess _data;

		public NewItemViewComponent(IConfiguration config) 
		{
			_data = new DataAccess(config);
		}

		public SelectList AppSelect { get; set; }
		public SelectList ProjectSelect { get; set; }

		public int ApplicationId { get; set; }
		public int? ProjectId { get; set; }
		public string Title { get; set; }

		public async Task<IViewComponentResult> InvokeAsync()
		{			
			using (var cn = _data.GetConnection())
			{
				_data.Initialize(User, TempData);
				AppSelect = await new AppSelect() { OrgId = _data.CurrentOrg.Id }.ExecuteSelectListAsync(cn);
				ProjectSelect = await new ProjectSelect() { OrgId = _data.CurrentOrg.Id }.ExecuteSelectListAsync(cn);
			}

			return View(this);
		}
	}
}
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class DataModelsModel : AppPageModel
	{
		public DataModelsModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty(SupportsGet = true)]
		public int AppId { get; set; }

		public SelectList AppSelect { get; set; }

		public IEnumerable<DataModel> DataModels { get; set; }

		public async Task OnGetAsync(bool isActive = true)
		{
			using (var cn = Data.GetConnection())
			{
				AppSelect = await new AppSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, AppId);
				DataModels = await new DataModels() { AppId = AppId, IsActive = isActive }.ExecuteAsync(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(DataModel record)
		{
			await Data.TrySaveAsync(record);
			return Redirect($"/Setup/DataModels?AppId={record.ApplicationId}");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			var dm = await Data.FindAsync<DataModel>(id);
			await Data.TryDelete<DataModel>(id);
			return Redirect($"/Setup/DataModels?AppId={dm.ApplicationId}");
		}
	}
}
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Data
{
	public class IndexModel : AppPageModel
	{
		public IndexModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty(SupportsGet = true)]
		public int ModelId { get; set; }

		[BindProperty(SupportsGet = true)]
		public int Id { get; set; }

		public DataModel CurrentModel { get; set; }
		public ModelClass CurrentClass { get; set; }

		public IEnumerable<SelectListItem> ModelSelect { get; set; }

		public IEnumerable<ModelClass> ModelClasses { get; set; }

		public async Task OnGetAsync()
		{			
			using (var cn = Data.GetConnection())
			{
				if (Id != 0)
				{
					CurrentClass = await Data.FindAsync<ModelClass>(Id);
					ModelId = CurrentClass.DataModelId;
				}

				CurrentModel = await Data.FindAsync<DataModel>(ModelId);
				ModelSelect = await new DataModelSelect() { AppId = CurrentOrgUser.CurrentAppId ?? 0 }.ExecuteSelectListAsync(cn, ModelId);
				ModelClasses = await new ModelClasses() { ModelId = ModelId }.ExecuteAsync(cn);
			}
		}

		public async Task<IActionResult> OnPostAddClassAsync(ModelClass record)
		{
			await Data.TrySaveAsync(record);
			return Redirect($"/Data?modelId={record.DataModelId}");
		}
	}
}
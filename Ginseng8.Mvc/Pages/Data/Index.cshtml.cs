using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
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
		public IEnumerable<ModelProperty> Properties { get; set; }
		public IEnumerable<ChildClassesResult> Children { get; set; }

		public IEnumerable<SelectListItem> ModelSelect { get; set; }
		public SelectList DataTypeSelect { get; set; }

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
				ModelClasses = await new ModelClasses() { ModelId = ModelId, IsScalar = false }.ExecuteAsync(cn);
				Properties = await new ModelProperties() { ClassId = Id }.ExecuteAsync(cn);
				Children = await new ChildClasses() { ParentClassId = Id }.ExecuteAsync(cn);

				ModelSelect = await new DataModelSelect() { AppId = CurrentOrgUser.CurrentAppId ?? 0 }.ExecuteSelectListAsync(cn, ModelId);
				var allTypes = await new ModelClasses() { ModelId = ModelId }.ExecuteAsync(cn);
				DataTypeSelect = new SelectList(allTypes.Select(mc => new SelectListItem() { Value = mc.Id.ToString(), Text = mc.Name }), "Value", "Text");
			}
		}

		public async Task<IActionResult> OnPostAddClassAsync(ModelClass record)
		{
			await Data.TrySaveAsync(record);
			return Redirect($"/Data/{record.Id}");
		}

		public async Task<IActionResult> OnPostSavePropertyAsync(ModelProperty record)
		{
			await Data.TrySaveAsync(record);
			return Redirect($"/Data/{record.ModelClassId}");
		}

		public async Task<IActionResult> OnPostDeletePropertyAsync(int id)
		{
			var mp = await Data.FindAsync<ModelProperty>(id);
			await Data.TryDelete<ModelProperty>(id);
			return Redirect($"/Data/{mp.ModelClassId}");
		}
	}
}
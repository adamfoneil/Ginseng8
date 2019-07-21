using Ginseng.Models;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class LabelInstructionsModel : AppPageModel
    {
        public LabelInstructionsModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int LabelId { get; set; }

        public SelectList LabelSelect { get; set; }

        [BindProperty]
        public LabelInstructions LabelInstructions { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                LabelSelect = await new LabelSelect() { OrgId = OrgId, AllowNewItems = true }.ExecuteSelectListAsync(cn, LabelId);

                if (LabelId != 0)
                {
                    LabelInstructions =
                        await Data.FindWhereAsync<LabelInstructions>(cn, new { LabelId }) ??
                        new LabelInstructions() { LabelId = LabelId };
                }
            }
        }

        public async Task<RedirectResult> OnPostSave(LabelInstructions record)
        {
            await Data.TrySaveAsync(record, successMessage: "Instructions saved successfully.");
            return Redirect($"/Setup/LabelInstructions?LabelId={record.LabelId}");
        }
    }
}
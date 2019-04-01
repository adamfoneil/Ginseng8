using Ginseng.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Wiki
{
	[Authorize]
	public class EditModel : WikiPageModel
	{
		public EditModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty]
		public Article Article { get; set; }

		[BindProperty(SupportsGet = true)]
		public int Id { get; set; }

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			Article = await Data.FindAsync<Article>(Id);
		}

		public async Task<IActionResult> OnPostAsync(Article article)
		{
			await Data.TrySaveAsync(article);
			return RedirectToPage("Index", new { id = article.Id });
		}
	}
}
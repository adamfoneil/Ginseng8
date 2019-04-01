using Ginseng.Models;
using Ginseng.Mvc.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	public class WikiPageModel : AppPageModel
	{
		public WikiPageModel(IConfiguration config) : base(config)
		{
		}

		public Node<Article> TableOfContents { get; set; }

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

		public async Task<IActionResult> OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				var articles = await new Articles() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);
				TableOfContents = Node<Article>.ResolveStructure(articles, (a) => a.Location, '/');

				await OnGetInternalAsync(cn);

				OnGetInternal(cn);
			}

			return Page();
		}
	}
}
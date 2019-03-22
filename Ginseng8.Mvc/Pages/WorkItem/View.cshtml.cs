using System.Data.SqlClient;
using System.Linq;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class ViewModel : DashboardPageModel
	{
		public ViewModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty(SupportsGet = true)]
		public int Id { get; set; }

		public OpenWorkItemsResult Item { get; set; }

		protected override void OnGetInternal(SqlConnection connection)
		{
			base.OnGetInternal(connection);

			Item = WorkItems?.First();
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems() { OrgId = OrgId, Number = Id, IsOpen = null };
		}
	}
}
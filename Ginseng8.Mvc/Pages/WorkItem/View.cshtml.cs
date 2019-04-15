using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class ViewModel : DashboardPageModel
	{
		private readonly BlobStorage _blobStorage;

		public ViewModel(IConfiguration config) : base(config)
		{
			_blobStorage = new BlobStorage(config);
		}

		/// <summary>
		/// This is really WorkItem.Number
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public int Id { get; set; }

		public OpenWorkItemsResult Item { get; set; }
		public IEnumerable<Attachment> Attachments { get; set; }

		protected override void OnGetInternal(SqlConnection connection)
		{
			base.OnGetInternal(connection);

			Item = WorkItems?.First();
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			Attachments = await new Attachments(QueryTraces) { OrgId = OrgId, ObjectType = ObjectType.WorkItem, ObjectId = Id, UserName = User.Identity.Name }.ExecuteAsync(connection);			
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems(QueryTraces) { OrgId = OrgId, Number = Id, IsOpen = null };
		}
	}
}
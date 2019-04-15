using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;

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
		public AttachmentsView Attachments { get; set; }

		protected override void OnGetInternal(SqlConnection connection)
		{
			base.OnGetInternal(connection);

			Item = WorkItems?.First();
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			var workItem = await connection.FindWhereAsync<Ginseng.Models.WorkItem>(new { OrganizationId = CurrentOrg.Id, Number = Id });
			var blobs = await _blobStorage.ListBlobs(CurrentOrg.Name, $"WorkItems/{Id}");			
			Attachments = new AttachmentsView()
			{
				AllowDelete = (workItem.CreatedBy.Equals(User.Identity.Name)),
				Blobs = blobs
			};
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems(QueryTraces) { OrgId = OrgId, Number = Id, IsOpen = null };
		}
	}
}
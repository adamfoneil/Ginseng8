using Dapper;
using Ginseng.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItems
{
	public class CreateModel : AppPageModel
	{
		public CreateModel(IConfiguration config) : base(config)
		{
		}

		public WorkItem WorkItem { get; private set; }

		public async Task OnPostAsync(WorkItem workItem)
		{
			workItem.OrganizationId = Data.CurrentOrg.Id;
			workItem.Number = await GetWorkItemNumberAsync();
			await Data.TrySaveAsync(workItem);
			WorkItem = workItem;
		}

		private async Task<int> GetWorkItemNumberAsync()
		{
			using (var cn = Data.GetConnection())
			{
				int result = await cn.QuerySingleAsync<int>(
					@"SELECT [NextWorkItemNumber] FROM [dbo].[Organization] WHERE [Id]=@orgId;
					UPDATE [dbo].[Organization] SET [NextWorkItemNumber]=[NextWorkItemNumber]+1 WHERE [Id]=@orgId", new { orgId = Data.CurrentOrg.Id });
				return result;
			}
		}
	}
}
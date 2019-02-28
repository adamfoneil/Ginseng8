using Ginseng.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	public class AppPageModel : PageModel
	{
		private IConfiguration _config;

		protected UserProfile CurrentUser;
		protected Organization CurrentOrg;

		public AppPageModel(IConfiguration config)
		{
			_config = config;
		}

		protected SqlConnection GetConnection()
		{
			string connectionStr = _config.GetConnectionString("DefaultConnection");
			return new SqlConnection(connectionStr);
		}

		public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
		{
			await base.OnPageHandlerExecutionAsync(context, next);

			using (var cn = GetConnection())
			{
				CurrentUser = await cn.FindWhereAsync<UserProfile>(new { userName = User.Identity.Name });
				if (CurrentUser.OrganizationId != null)
				{
					CurrentOrg = await cn.FindAsync<Organization>(CurrentUser.OrganizationId.Value);
				}
			}
		}


		protected async Task<T> FindAsync<T>(int id)
		{
			using (var cn = GetConnection())
			{
				return await cn.FindAsync<T>(id);
			}
		}
	}
}
using Ginseng.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	public enum SaveMessageType
	{		
		Success,
		Error
	}

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

		[TempData]
		public SaveMessageType SaveMessageType { get; set; }

		[TempData]
		public string SaveMessage { get; set; }

		private static Dictionary<SaveMessageType, string> AlertCssClasses =>
			new Dictionary<SaveMessageType, string>()
			{
				{ SaveMessageType.Success, "alert-success" },
				{ SaveMessageType.Error, "alert-danger" }
			};

		public string AlertClass
		{
			get { return (!string.IsNullOrEmpty(SaveMessage)) ? AlertCssClasses[SaveMessageType] : null; }
		}

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			base.OnPageHandlerExecuting(context);

			using (var cn = GetConnection())
			{
				GetCurrentUser(cn);
			}
		}

		private void GetCurrentUser(SqlConnection cn)
		{
			CurrentUser = cn.FindWhere<UserProfile>(new { userName = User.Identity.Name });
			if (CurrentUser.OrganizationId != null)
			{
				CurrentOrg = cn.Find<Organization>(CurrentUser.OrganizationId.Value);
			}
		}

		protected async Task<T> FindAsync<T>(int id)
		{
			using (var cn = GetConnection())
			{				
				return await cn.FindAsync<T>(id, CurrentUser);
			}
		}

		protected async Task<bool> TrySaveAsync<T>(T record, string successMessage = null)
		{
			try
			{
				using (var cn = GetConnection())
				{				
					await cn.SaveAsync(record, CurrentUser);
					if (!string.IsNullOrEmpty(successMessage))
					{
						SaveMessageType = SaveMessageType.Success;
						SaveMessage = successMessage;
					}
					return true;
				}
			}
			catch (Exception exc)
			{
				SaveMessageType = SaveMessageType.Error;
				SaveMessage = exc.Message;
				return false;
			}
		}
	}
}
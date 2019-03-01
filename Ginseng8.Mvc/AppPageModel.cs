using Ginseng.Models;
using Ginseng.Models.Conventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
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
		protected OrganizationUser CurrentOrgUser;

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
				CurrentOrgUser = cn.FindWhere<OrganizationUser>(new { organizationId = CurrentUser.OrganizationId.Value, userId = CurrentUser.UserId });
			}
		}

		protected async Task<T> FindAsync<T>(int id)
		{
			using (var cn = GetConnection())
			{				
				return await cn.FindAsync<T>(id, CurrentUser);
			}
		}

		protected async Task<bool> TrySaveAsync<T>(T record, string[] propertyNames, string successMessage = null) where T : BaseTable
		{
			try
			{
				using (var cn = GetConnection())
				{
					var update = AuditProperties(record, propertyNames);					
					await cn.SaveAsync(update.Item1, update.Item2);
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

		/// <summary>
		/// Sets the audit tracking fields when we're updating dynamic columns
		/// </summary>
		private (T, string[]) AuditProperties<T>(T record, string[] propertyNames) where T : BaseTable
		{
			var properties = propertyNames.ToList();

			if (record.Id == 0)
			{
				record.CreatedBy = User.Identity.Name;
				record.DateCreated = CurrentUser.LocalTime;
				properties.Add(nameof(BaseTable.CreatedBy));
				properties.Add(nameof(BaseTable.DateCreated));
			}
			else
			{
				record.ModifiedBy = User.Identity.Name;
				record.DateModified = CurrentUser.LocalTime;
				properties.Add(nameof(BaseTable.ModifiedBy));
				properties.Add(nameof(BaseTable.DateModified));
			}
			
			return (record, properties.ToArray());
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

		protected async Task<bool> TryUpdateAsync<T>(T record, params Expression<Func<T, object>>[] setColumns)
		{
			try
			{
				using (var cn = GetConnection())
				{
					await cn.UpdateAsync(record, CurrentUser, setColumns);
					SaveMessageType = SaveMessageType.Success;
					SaveMessage = "Record was updated successfully.";
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
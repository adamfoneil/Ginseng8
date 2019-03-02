using Ginseng.Models;
using Ginseng.Models.Conventions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	/// <summary>
	/// Low-level CRUD handler that supports Pages and ViewComponents
	/// </summary>
	public class DataAccess
	{
		private readonly IConfiguration _config;				

		public UserProfile CurrentUser { get; private set; }
		public Organization CurrentOrg { get; private set; }
		public OrganizationUser CurrentOrgUser { get; private set; }

		public DataAccess(IConfiguration config)
		{			
			_config = config;
		}

		public SqlConnection Open()
		{
			string connectionStr = _config.GetConnectionString("DefaultConnection");
			return new SqlConnection(connectionStr);
		}

		public ITempDataDictionary TempData { get; private set; }

		public void Initialize(ClaimsPrincipal user, ITempDataDictionary tempData)
		{
			TempData = tempData;

			using (var cn = Open())
			{
				CurrentUser = cn.FindWhere<UserProfile>(new { userName = user.Identity.Name });
				if (CurrentUser.OrganizationId != null)
				{
					CurrentOrg = cn.Find<Organization>(CurrentUser.OrganizationId.Value);
					CurrentOrgUser = cn.FindWhere<OrganizationUser>(new { organizationId = CurrentUser.OrganizationId.Value, userId = CurrentUser.UserId });
				}
			}
		}

		public async Task<T> FindAsync<T>(int id)
		{
			using (var cn = Open())
			{
				return await cn.FindAsync<T>(id, CurrentUser);
			}
		}

		public async Task<bool> TrySaveAsync<T>(T record, string[] propertyNames, string successMessage = null) where T : BaseTable
		{
			try
			{
				using (var cn = Open())
				{
					var update = AuditProperties(record, propertyNames);
					await cn.SaveAsync(update.Item1, update.Item2);
					SetSuccessMessage(successMessage);
					return true;
				}
			}
			catch (Exception exc)
			{
				SetErrorMessage(exc);
				return false;
			}
		}

		public async Task<bool> TrySaveAsync<T>(T record, string successMessage = null)
		{
			try
			{
				using (var cn = Open())
				{
					await cn.SaveAsync(record, CurrentUser);
					SetSuccessMessage(successMessage);
					return true;
				}
			}
			catch (Exception exc)
			{
				SetErrorMessage(exc);
				return false;
			}
		}

		public async Task<bool> TryUpdateAsync<T>(T record, params Expression<Func<T, object>>[] setColumns)
		{
			try
			{
				using (var cn = Open())
				{
					await cn.UpdateAsync(record, CurrentUser, setColumns);					
					return true;
				}
			}
			catch (Exception exc)
			{
				SetErrorMessage(exc);
				return false;
			}
		}

		public async Task<bool> TryDelete<T>(int id, string successMessage = null)
		{
			try
			{
				using (var cn = Open())
				{
					await cn.DeleteAsync<T>(id, CurrentUser);
					SetSuccessMessage(successMessage);
					return true;
				}
			}
			catch (Exception exc)
			{
				SetErrorMessage(exc);
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
				record.CreatedBy = CurrentUser.UserName;
				record.DateCreated = CurrentUser.LocalTime;
				properties.Add(nameof(BaseTable.CreatedBy));
				properties.Add(nameof(BaseTable.DateCreated));
			}
			else
			{
				record.ModifiedBy = CurrentUser.UserName;
				record.DateModified = CurrentUser.LocalTime;
				properties.Add(nameof(BaseTable.ModifiedBy));
				properties.Add(nameof(BaseTable.DateModified));
			}

			return (record, properties.ToArray());
		}

		private void SetSuccessMessage(string message)
		{
			TempData.Remove(AlertCss.Success);
			if (string.IsNullOrEmpty(message)) return;
			TempData.Add(AlertCss.Success, message);
		}

		private void SetErrorMessage(Exception exception)
		{
			TempData.Remove(AlertCss.Error);
			TempData.Add(AlertCss.Error, exception.Message);
		}
	}
}
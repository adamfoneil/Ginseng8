using Ginseng.Models;
using Ginseng.Models.Conventions;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
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
		private readonly ClaimsPrincipal _user;

		public UserProfile CurrentUser { get; private set; }
		public Organization CurrentOrg { get; private set; }
		public OrganizationUser CurrentOrgUser { get; private set; }

		public ActionMessage ActionMessage { get; private set; }

		public DataAccess(ClaimsPrincipal user, IConfiguration config)
		{
			_user = user;
			_config = config;
		}

		public string OrgName => CurrentOrg?.Name ?? "(no org)";

		public SqlConnection GetConnection()
		{
			string connectionStr = _config.GetConnectionString("DefaultConnection");
			return new SqlConnection(connectionStr);
		}

		public void GetCurrentUser()
		{
			using (var cn = GetConnection())
			{
				CurrentUser = cn.FindWhere<UserProfile>(new { userName = _user.Identity.Name });
				if (CurrentUser.OrganizationId != null)
				{
					CurrentOrg = cn.Find<Organization>(CurrentUser.OrganizationId.Value);
					CurrentOrgUser = cn.FindWhere<OrganizationUser>(new { organizationId = CurrentUser.OrganizationId.Value, userId = CurrentUser.UserId });
				}
			}
		}

		public async Task<T> FindAsync<T>(int id)
		{
			using (var cn = GetConnection())
			{
				return await cn.FindAsync<T>(id, CurrentUser);
			}
		}

		public async Task<bool> TrySaveAsync<T>(T record, string[] propertyNames, string successMessage = null) where T : BaseTable
		{
			try
			{
				using (var cn = GetConnection())
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
				using (var cn = GetConnection())
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
				using (var cn = GetConnection())
				{
					await cn.UpdateAsync(record, CurrentUser, setColumns);
					ActionMessage = null;
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
				using (var cn = GetConnection())
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

		private void SetErrorMessage(Exception exception)
		{
			SetMessage(ActionMessageType.Error, exception.Message);
		}

		private void SetSuccessMessage(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				ActionMessage = null;
			}
			else
			{
				SetMessage(ActionMessageType.Success, message);
			}
		}

		private void SetMessage(ActionMessageType type, string message)
		{
			ActionMessage = new ActionMessage()
			{
				Type = type,
				Message = message
			};
		}

		/// <summary>
		/// Sets the audit tracking fields when we're updating dynamic columns
		/// </summary>
		private (T, string[]) AuditProperties<T>(T record, string[] propertyNames) where T : BaseTable
		{
			var properties = propertyNames.ToList();

			if (record.Id == 0)
			{
				record.CreatedBy = _user.Identity.Name;
				record.DateCreated = CurrentUser.LocalTime;
				properties.Add(nameof(BaseTable.CreatedBy));
				properties.Add(nameof(BaseTable.DateCreated));
			}
			else
			{
				record.ModifiedBy = _user.Identity.Name;
				record.DateModified = CurrentUser.LocalTime;
				properties.Add(nameof(BaseTable.ModifiedBy));
				properties.Add(nameof(BaseTable.DateModified));
			}

			return (record, properties.ToArray());
		}
	}
}

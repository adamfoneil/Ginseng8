using Ginseng.Models;
using Ginseng.Models.Conventions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
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

        public string UserDisplayName
        {
            get { return CurrentOrgUser?.DisplayName ?? CurrentUser.UserName; }
        }

        public DataAccess(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection GetConnection()
        {
            string connectionStr = _config.GetSection("ConnectionStrings").GetValue<string>("Default");
            return new SqlConnection(connectionStr);
        }

        public ITempDataDictionary TempData { get; private set; }

        public void Initialize(SqlConnection connection, IPrincipal user, ITempDataDictionary tempData)
        {
            TempData = tempData;

            CurrentUser = connection.FindWhere<UserProfile>(new { userName = user.Identity.Name });
            if (CurrentUser.OrganizationId != null)
            {
                CurrentOrg = connection.Find<Organization>(CurrentUser.OrganizationId.Value);
                CurrentOrgUser = connection.FindWhere<OrganizationUser>(
                    new { organizationId = CurrentUser.OrganizationId.Value, userId = CurrentUser.UserId }) ??
                    new OrganizationUser() { OrganizationId = CurrentUser.OrganizationId.Value, UserId = CurrentUser.UserId };
            }
        }

        public void Initialize(IPrincipal user, ITempDataDictionary tempData)
        {
            using (var cn = GetConnection())
            {
                Initialize(cn, user, tempData);
            }
        }

        public async Task<WorkItem> FindWorkItemAsync(SqlConnection connection, int number)
        {
            return await FindWhereAsync<WorkItem>(connection, new { OrganizationId = CurrentOrg.Id, Number = number });
        }

        public async Task<T> FindWhereAsync<T>(SqlConnection connection, object criteria)
        {
            return await connection.FindWhereAsync<T>(criteria, CurrentUser);
        }

        public async Task<T> FindWhereAsync<T>(object criteria)
        {
            using (var cn = GetConnection())
            {
                return await cn.FindWhereAsync<T>(criteria, CurrentUser);
            }
        }

        public async Task<T> FindAsync<T>(SqlConnection connection, int id)
        {
            return await connection.FindAsync<T>(id, CurrentUser);
        }

        public async Task<T> FindAsync<T>(int id)
        {
            using (var cn = GetConnection())
            {
                return await FindAsync<T>(cn, id);
            }
        }

        public async Task<bool> TrySaveAsync<T>(SqlConnection connection, T record, string[] propertyNames, string successMessage = null) where T : BaseTable
        {
            try
            {
                var update = AuditProperties(record, propertyNames);
                await connection.SaveAsync(update.Item1, update.Item2);
                SetSuccessMessage(successMessage);
                return true;
            }
            catch (Exception exc)
            {
                SetErrorMessage(exc);
                return false;
            }
        }

        public async Task<bool> TrySaveAsync<T>(T record, string[] propertyNames, string successMessage = null) where T : BaseTable
        {
            using (var cn = GetConnection())
            {
                cn.Open();
                return await TrySaveAsync(cn, record, propertyNames, successMessage);
            }
        }

        public async Task<bool> TrySaveAsync<T>(SqlConnection connection, T record, Action<SqlConnection, T> beforeSave = null, string successMessage = null)
        {
            try
            {
                beforeSave?.Invoke(connection, record);
                if (connection.State == ConnectionState.Closed) connection.Open();
                await connection.SaveAsync(record, CurrentUser);
                SetSuccessMessage(successMessage);
                return true;
            }
            catch (Exception exc)
            {
                SetErrorMessage(exc);
                return false;
            }
        }

        public async Task<bool> TrySaveAsync<T>(T record, Action<SqlConnection, T> beforeSave = null, string successMessage = null)
        {
            try
            {
                using (var cn = GetConnection())
                {
                    cn.Open();
                    return await TrySaveAsync(cn, record, beforeSave, successMessage);
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
                    return true;
                }
            }
            catch (Exception exc)
            {
                SetErrorMessage(exc);
                return false;
            }
        }

        public async Task<bool> TryDeleteAsync<T>(SqlConnection connection, int id, string successMessage = null)
        {
            try
            {
                await connection.DeleteAsync<T>(id, CurrentUser);
                SetSuccessMessage(successMessage);
                return true;

            }
            catch (Exception exc)
            {
                SetErrorMessage(exc);
                return false;
            }
        }


        public async Task<bool> TryDeleteAsync<T>(int id, string successMessage = null)
        {
            using (var cn = GetConnection())
            {
                return await TryDeleteAsync<T>(cn, id, successMessage);
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

        public void SetSuccessMessage(string message)
        {
            TempData.Remove(AlertCss.Success);
            if (string.IsNullOrEmpty(message)) return;
            TempData.Add(AlertCss.Success, message);
        }

        public void SetErrorMessage(string message)
        {
            TempData.Remove(AlertCss.Error);
            TempData.Add(AlertCss.Error, message);
        }

        public void SetErrorMessage(Exception exception)
        {
            TempData.Remove(AlertCss.Error);
            TempData.Add(AlertCss.Error, exception.Message);
        }

        public void ClearErrorMessage()
        {
            TempData.Remove(AlertCss.Error);
        }
    }
}
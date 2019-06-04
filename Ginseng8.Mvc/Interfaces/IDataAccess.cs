namespace Ginseng.Mvc.Interfaces
{
    /// <summary>
    /// Data access service interface
    /// </summary>
    public interface IDataAccess
    {
        /*
        UserProfile CurrentUser { get; }
        Organization CurrentOrg { get; }
        OrganizationUser CurrentOrgUser { get; }
        string UserDisplayName { get; }
        ITempDataDictionary TempData { get; }

        SqlConnection GetConnection();

        void Initialize(SqlConnection connection, IPrincipal user, ITempDataDictionary tempData);
        void Initialize(IPrincipal user, ITempDataDictionary tempData);

        Task<T> FindWhereAsync<T>(SqlConnection connection, object criteria);
        Task<T> FindWhereAsync<T>(object criteria);
        Task<T> FindAsync<T>(SqlConnection connection, int id);
        Task<T> FindAsync<T>(int id);

        Task<bool> TrySaveAsync<T>(SqlConnection connection, T record, string[] propertyNames, string successMessage = null) where T : BaseTable;
        Task<bool> TrySaveAsync<T>(T record, string[] propertyNames, string successMessage = null) where T : BaseTable;
        Task<bool> TrySaveAsync<T>(SqlConnection connection, T record, Action<SqlConnection, T> beforeSave = null, string successMessage = null);
        Task<bool> TrySaveAsync<T>(T record, Action<SqlConnection, T> beforeSave = null, string successMessage = null);
        Task<bool> TryUpdateAsync<T>(T record, params Expression<Func<T, object>>[] setColumns);
        Task<bool> TryDeleteAsync<T>(int id, string successMessage = null);

        void SetSuccessMessage(string message);
        void SetErrorMessage(string message);
        void SetErrorMessage(Exception exception);
        void ClearErrorMessage();
        */
    }
}

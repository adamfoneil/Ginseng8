using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models.Interfaces
{
    /// <summary>
    /// Implement this model classes to provide a standard way of accessing a record's containing OrganizationId.
    /// This is how tenant isolation is checked in <see cref="Models.Conventions.BaseTable.CheckOrgPermissionAsync(IDbConnection, Postulate.Base.Interfaces.IUser)"/>
    /// </summary>
    public interface IOrgSpecific
    {
        Task<int> GetOrgIdAsync(IDbConnection connection);
    }
}
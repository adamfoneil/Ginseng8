using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Ginseng.Models.Interfaces
{
    /// <summary>
    /// Implement this model classes to provide a standard way of accessing a record's containing OrganizationId.
    /// This is how tenant isolation is checked by 
    /// </summary>
    public interface IOrgSpecific
    {
        Task<int> GetOrgIdAsync(IDbConnection connection);
    }
}

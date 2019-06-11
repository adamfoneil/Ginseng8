using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class Team : BaseTable, IOrgSpecific
    {
        [References(typeof(Organization))]
        [PrimaryKey]
        public int OrganizationId { get; set; }

        [PrimaryKey]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        /// <summary>
        /// Select Freshdesk companies when creating apps (i.e. treat apps as customers)
        /// </summary>
        [DefaultExpression("0")]
        public bool AppsFromCompanies { get; set; }

        public bool IsActive { get; set; } = true;

        public Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return Task.FromResult(OrganizationId);
        }
    }
}
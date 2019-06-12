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
        /// Indicates whether the Applications dashboard is shown.
        /// If false, then the team uses one generic hidden application
        /// to attach their milestones and projects
        /// </summary>
        public bool UseApplications { get; set; }

        /// <summary>
        /// Activates the Freshdesk company selector on the project edit page
        /// </summary>
        public bool CompanySpecificProjects { get; set; }

        public bool IsActive { get; set; } = true;

        public Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return Task.FromResult(OrganizationId);
        }
    }
}
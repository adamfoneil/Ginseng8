using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class Team : BaseTable, IOrgSpecific, ISelectable, IFindRelated<int>
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

        [NotMapped]
        public bool Selected { get; set; }

        [NotMapped]
        public int LabelId { get; set; }

        [NotMapped]
        public int InactiveProjects { get; set; }

        [NotMapped]
        public int ActiveProjects { get; set; }

        /// <summary>
        /// For ISelectable
        /// </summary>
        [NotMapped]
        public string ForeColor { get; set; } = "auto";

        [NotMapped]
        public string BackColor { get; set; } = "auto";

        public Organization Organization { get; set; }

        public override bool Equals(object obj)
        {
            var test = obj as Team;
            return (test != null) ? test.Id == Id : false;
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Organization = commandProvider.Find<Organization>(connection, OrganizationId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Organization = await commandProvider.FindAsync<Organization>(connection, OrganizationId);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return Task.FromResult(OrganizationId);
        }
    }
}
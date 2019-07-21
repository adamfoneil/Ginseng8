using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// A descriptor such as Bug or Feature or some other wording that helps categorize work items
    /// </summary>
    public class Label : BaseTable, IOrgSpecific, ISelectable
    {
        public const string PlaceholderLabel = "placeholder";

        [References(typeof(Organization))]
        [PrimaryKey]
        public int OrganizationId { get; set; }

        [MaxLength(50)]
        [PrimaryKey]
        public string Name { get; set; }

        [MaxLength(50)]
        public string CssClass { get; set; }

        [MaxLength(50)]
        public string BackColor { get; set; }

        [MaxLength(50)]
        public string ForeColor { get; set; }

        /// <summary>
        /// Show this label on Dashboard/New for entering new items
        /// </summary>
        [DefaultExpression("0")]
        public bool AllowNewItems { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// For queries only
        /// </summary>
        [NotMapped]
        public int WorkItemId { get; set; }

        [NotMapped]
        public int ApplicationId { get; set; }

        /// <summary>
        /// For queries only
        /// </summary>
        [NotMapped]
        public bool Selected { get; set; }

        [NotMapped]
        public int TeamId { get; set; }

        public override bool Equals(object obj)
        {
            Label test = obj as Label;
            return (test?.Id == Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }
}
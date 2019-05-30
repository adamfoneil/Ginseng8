using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// A software product managed by an organization
	/// </summary>
	public class Application : BaseTable, IOrgSpecific
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
		/// App's live URL, as applicable
		/// </summary>
		[MaxLength(255)]
		public string Url { get; set; }

        /// <summary>
        /// Where do we email invoices?
        /// </summary>
        [MaxLength(100)]
        public string InvoiceEmail { get; set; }            

        /// <summary>
        /// Show this app on the New Items dashbord page
        /// </summary>
        [DefaultExpression("1")]
        public bool AllowNewItems { get; set; }

		public bool IsActive { get; set; } = true;

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }
}
using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// A software product managed by an organization
	/// </summary>
	public class Application : BaseTable
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

		public bool IsActive { get; set; } = true;
	}
}
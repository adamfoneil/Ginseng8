using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	/// <summary>
	/// A descriptor such as Bug or Feature or some other wording that helps categorize work items
	/// </summary>
	public class Label : BaseTable
	{
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

		public bool IsActive { get; set; } = true;

		/// <summary>
		/// For queries only
		/// </summary>
		[NotMapped]
		public int WorkItemId { get; set; }

		/// <summary>
		/// For queries only
		/// </summary>
		[NotMapped]
		public bool Selected { get; set; }

		public override bool Equals(object obj)
		{
			Label test = obj as Label;
			return (test?.Id == Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
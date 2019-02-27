using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	public class Activity : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[References(typeof(Responsibility))]
		public int ResponsibilityId { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		public bool IsActive { get; set; } = true;
	}
}
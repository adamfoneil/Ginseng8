using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	public class Organization : BaseTable
	{
		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[References(typeof(UserProfile))]
		public int OwnerUserId { get; set; }

		[DefaultExpression("1000")]
		public int NextWorkItemNumber { get; set; } = 1000;
	}
}
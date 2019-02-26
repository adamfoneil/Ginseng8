using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	public class Task : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[ColumnAccess(SaveAction.Insert)]
		public int Number { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }
	}
}
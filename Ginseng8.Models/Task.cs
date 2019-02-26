using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	public class Task : BaseTable, IBody
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		[ColumnAccess(SaveAction.Insert)]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[ColumnAccess(SaveAction.Insert)]
		public int Number { get; set; }

		[MaxLength(255)]
		public string Title { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		[References(typeof(Application))]
		public int ApplicationId { get; set; }

		[References(typeof(Activity))]
		public int? ActivityId { get; set; }

		[References(typeof(Milestone))]
		public int? MilestoneId { get; set; }
	}
}
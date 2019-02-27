using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	/// <summary>
	/// Description of work scope and estimate for a work item, and the assignment of a developer to a task
	/// </summary>
	public class Development : BaseTable, IStatusUpdate, IBody
	{
		[References(typeof(WorkItem))]
		[PrimaryKey]
		public int WorkItemId { get; set; }

		public string IconClass => "tools";

		/// <summary>
		/// Developer who owns this item
		/// </summary>
		[References(typeof(UserProfile))]
		public int UserId { get; set; }

		/// <summary>
		/// Broad estimate of amount of work involved (impact)
		/// </summary>
		[References(typeof(WorkItemSize))]
		public int? SizeId { get; set; }

		[Column(TypeName = "date")]
		public DateTime? TargetDate { get; set; }

		public string HtmlBody { get; set; }

		public string TextBody { get; set; }
	}
}
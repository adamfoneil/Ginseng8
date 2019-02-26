using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// How much work is expected for this task?
	/// </summary>
	public class Estimate : BaseTable, ITaskUpdate, IBody
	{
		[References(typeof(Task))]
		[PrimaryKey]
		public int TaskId { get; set; }

		public string IconClass => "estimate";

		public string HtmlBody { get; set; }

		public string TextBody { get; set; }

		public int? Hours { get; set; }

		/// <summary>
		/// Multiplier reflecting the unknowns and dependencies impacting the work on this task.
		/// For example a task of 8 hrs with uncertainty of 3, yields a "worst case" estimate of 24 hrs
		/// </summary>
		public decimal UncertaintyFactor { get; set; }
	}
}
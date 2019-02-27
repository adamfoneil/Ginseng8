using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// Indicates blockage on a work item,
	/// for example requirements issues, dependencies, cost implications, design options, testing failure
	/// </summary>
	public class Impediment : BaseTable, IBody, IFeedItem
	{
		[References(typeof(WorkItem))]
		[PrimaryKey]
		public int WorkItemId { get; set; }

		[References(typeof(Activity))]
		[PrimaryKey]
		public int ActivityId { get; set; }

		public string IconClass => "hand";

		/// <summary>
		/// Indicates whether the blockage is in effect or not
		/// </summary>
		public bool IsActive { get; set; } = true;

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }
	}
}
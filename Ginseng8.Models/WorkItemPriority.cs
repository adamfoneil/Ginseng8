using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{	
	[TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
	public class WorkItemPriority : BaseTable
	{
		[References(typeof(WorkItem))]
		[PrimaryKey]
		public int WorkItemId { get; set; }
		
		public int UserId { get; set; }

		public int Value { get; set; }
	}
}
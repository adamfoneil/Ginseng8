using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{	
	public class WorkItemPriority : BaseTable
	{
		[References(typeof(WorkItem))]
		[PrimaryKey]
		public int WorkItemId { get; set; }

		[References(typeof(UserProfile))]
		public int UserId { get; set; }

		public int Value { get; set; }
	}
}
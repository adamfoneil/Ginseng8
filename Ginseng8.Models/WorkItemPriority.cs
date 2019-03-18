using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{		
	public class WorkItemPriority : BaseTable
	{
		[References(typeof(WorkItem))]
		[PrimaryKey]
		public int WorkItemId { get; set; }
	
		/// <summary>
		/// Can be a milestone Id or 0 for backlog
		/// </summary>
		public int MilestoneId { get; set; }

		/// <summary>
		/// Can be a user Id or 0 for overall business priority (irrespective of user)
		/// </summary>
		public int UserId { get; set; }

		public int Value { get; set; }
	}
}
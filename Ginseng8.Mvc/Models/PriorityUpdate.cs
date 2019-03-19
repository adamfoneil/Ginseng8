namespace Ginseng.Mvc.Models
{
	public class PriorityUpdate
	{
		/// <summary>
		/// Milestone that item was dragged onto
		/// </summary>
		public int MilestoneId { get; set; }

		/// <summary>
		/// User that item was dragged onto
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// Work item that was dragged
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// List of items in the milestone + user combination drop area after drag-dropping
		/// </summary>
		public Item[] Items { get; set; }
	}

	public class Item
	{
		/// <summary>
		/// Work item number
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// Position after drag-drop
		/// </summary>
		public int Index { get; set; }
	}
}
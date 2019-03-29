namespace Ginseng.Mvc.Models
{
	/// <summary>
	/// General-purpose type to represent anything placed in an Index order.
	/// I should've used this with <see cref="PriorityUpdate"/>, but at the moment
	/// I don't feel like going back to rework
	/// </summary>
	public class OrderedItem
	{
		public int Id { get; set; }
		public int Index { get; set; }
	}
}
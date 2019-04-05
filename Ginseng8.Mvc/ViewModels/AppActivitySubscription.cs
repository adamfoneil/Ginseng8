namespace Ginseng.Mvc.ViewModels
{
	public class AppActivitySubscription
	{
		public int AppId { get; set; }
		public string AppName { get; set; }
		public int ActivityId { get; set; }
		public string ActivityName { get; set; }
		public bool IsSubscribed { get; set; }
	}
}
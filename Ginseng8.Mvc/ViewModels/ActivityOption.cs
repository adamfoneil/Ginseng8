using Ginseng.Models;

namespace Ginseng.Mvc.ViewModels
{
	/// <summary>
	/// Used in Card hand-off controls to show options with forward and backward icons
	/// </summary>
	public class ActivityOption
	{
		public ActivityOption(Activity activity, bool isForward)
		{
			Activity = activity;
			IsForward = isForward;
		}

		public Activity Activity { get; }
		public bool IsForward { get; }

		public string FontawesomeClass()
		{
			return (IsForward) ?
				"fas fa-chevron-circle-right" :
				"far fa-chevron-circle-left";
		}

		public string Color()
		{
			return (IsForward) ? "green" : "orange";
		}
	}
}
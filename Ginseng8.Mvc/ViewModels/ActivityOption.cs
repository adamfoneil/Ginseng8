using Ginseng.Models;

namespace Ginseng.Mvc.ViewModels
{
	/// <summary>
	/// Used in Card hand-off controls to show options with forward and backward icons
	/// </summary>
	public class ActivityOption
	{
		public ActivityOption(int number, Activity activity, bool isForward)
		{
			Number = number;
			Activity = activity;
			IsForward = isForward;
		}

		public int Number { get; }
		public Activity Activity { get; }
		public bool IsForward { get; }

		/// <summary>
		/// If true, then it means work item assigned user is removed and,
		/// which means that info must be supplied for HandOff record
		/// such as test instructions, testing feedback, deployment notes, etc.
		/// If false, then it means the work item assigned user is set for the first time, so no info is required
		/// </summary>
		public bool IsHandOff { get; set; }		

		/// <summary>
		/// Activity option anchor tag Url
		/// </summary>		
		public string Url(string returnUrl)
		{
			// this navigates to page where the hand-off info is entered
			if (IsHandOff) return $"/WorkItem/HandOff/{Number}?activityId={Activity.Id}&returnUrl={returnUrl}";

			// if not a hand-off, then it will call a JS event handler (self-start-activity)
			return "#";
		}

		public string ClassName()
		{
			//Dashboard.js (line 140) has an event handler for this class which assigns the current user to this activity
			if (!IsHandOff) return "self-start-activity";
			return null;
		}

		public string FontawesomeClass()
		{
			return (IsForward) ?
				HandOff.ForwardHandOff :
				HandOff.BackwardHandOff;
		}
	}
}
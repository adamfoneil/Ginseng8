using System.Collections.Generic;

namespace Ginseng.Mvc
{
	public enum ActionMessageType
	{
		Success,
		Error
	}

	public class ActionMessage
	{
		public ActionMessageType Type { get; set; }
		public string Message { get; set; }

		public static Dictionary<ActionMessageType, string> CssClasses =>
			new Dictionary<ActionMessageType, string>()
			{
				{ ActionMessageType.Success, "alert-success" },
				{ ActionMessageType.Error, "alert-danger" }
			};
	}
}
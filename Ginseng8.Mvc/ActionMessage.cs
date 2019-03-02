using System.Collections.Generic;

namespace Ginseng.Mvc
{
	public enum ActionMessageType
	{
		Success,
		Error,
		Info
	}

	public class ActionMessage
	{
		public ActionMessageType Type { get; set; }
		public string Content { get; set; }

		public string CssClass
		{
			get { return CssClasses[Type]; }
		}

		private static Dictionary<ActionMessageType, string> CssClasses =>
			new Dictionary<ActionMessageType, string>()
			{
				{ ActionMessageType.Success, "alert-success" },
				{ ActionMessageType.Error, "alert-danger" },
				{ ActionMessageType.Info, "alert-info" }
			};
	}
}
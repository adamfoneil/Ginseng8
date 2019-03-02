using System.Linq;

namespace Ginseng.Mvc
{
	public static class AlertCss
	{
		public const string Success = "success";
		public const string Error = "danger";
		public const string Info = "info";

		public static string[] AllMessageTypes => new string[] { Success, Error, Info };		
	}
}
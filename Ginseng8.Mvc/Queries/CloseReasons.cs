using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class CloseReasons : Query<CloseReason>
	{
		public CloseReasons() : base("SELECT * FROM [app].[CloseReason] ORDER BY [Name]")
		{
		}
	}
}
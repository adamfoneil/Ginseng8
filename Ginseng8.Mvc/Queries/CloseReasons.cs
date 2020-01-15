using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class CloseReasons : Query<CloseReason>
	{
		public CloseReasons() : base("SELECT * FROM [app].[CloseReason] ORDER BY [Name]")
		{
		}
	}
}
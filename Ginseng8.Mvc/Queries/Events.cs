using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class Events : Query<Event>
	{
		public Events() : base("SELECT * FROM [app].[Event] ORDER BY [Name]")
		{
		}
	}
}
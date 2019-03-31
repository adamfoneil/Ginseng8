using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Events : Query<Event>
	{
		public Events() : base("SELECT * FROM [app].[Event] ORDER BY [Name]")
		{
		}
	}
}
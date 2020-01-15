using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class Responsibilities : Query<Responsibility>
	{
		public Responsibilities() : base("SELECT * FROM [app].[Responsibility] ORDER BY [Name]")
		{
		}
	}
}
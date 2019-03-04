using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Responsibilities : Query<Responsibility>
	{
		public Responsibilities() : base("SELECT * FROM [app].[Responsibility] ORDER BY [Name]")
		{
		}
	}
}
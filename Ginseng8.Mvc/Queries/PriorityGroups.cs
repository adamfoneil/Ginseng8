using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class PriorityGroups : Query<PriorityGroup>
	{
		public PriorityGroups() : base("SELECT * FROM [app].[PriorityGroup] ORDER BY [Id]")
		{
		}
	}
}
using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class PriorityGroups : Query<PriorityGroup>
	{
		public PriorityGroups() : base("SELECT * FROM [app].[PriorityGroup] ORDER BY [Id]")
		{
		}
	}
}
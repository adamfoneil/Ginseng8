using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	public class TaskLabel : BaseTable
	{
		[References(typeof(Task))]
		[PrimaryKey]
		public int TaskId { get; set; }

		[References(typeof(Label))]
		[PrimaryKey]
		public int LabelId { get; set; }
	}
}
using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	public class Event : AppTable
	{
		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }
	}
}
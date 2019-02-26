using Ginseng8.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ginseng8.Models
{
	public class Organization : BaseTable
	{
		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[References(typeof(UserProfile))]
		public int OwnerUserId { get; set; }
	}
}

using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// Defines some type of work that someone can do on a work item
	/// </summary>
	public class Activity : BaseTable, IFindRelated<int>
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		[References(typeof(Responsibility))]
		public int ResponsibilityId { get; set; }

		/// <summary>
		/// Overall order of activity
		/// </summary>
		public int Order { get; set; }

		public bool IsActive { get; set; } = true;

		public Responsibility Responsibility { get; set; }

		public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			Responsibility = commandProvider.Find<Responsibility>(connection, ResponsibilityId);
		}

		public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			Responsibility = await commandProvider.FindAsync<Responsibility>(connection, ResponsibilityId);
		}
	}
}
using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
	/// <summary>
	/// Defines the distinction between Business and Development work item responsibility.
	/// As work items change activity, the responsibility changes, which forms the bases of a CASE statement
	/// in the AllWorkItems query that shows the assigned person
	/// </summary>
	public class Responsibility : AppTable
	{
		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		/// <summary>
		/// Column expression that's valid within the AllWorkItems query that shows the assigned person		
		/// </summary>
		public string SourceExpression { get; set; }

		[DefaultExpression("0")]
		public int Flag { get; set; }

		public static DataTable GetSeedData()
		{
			return new Responsibility[]
			{
				new Responsibility() { Name = "Business", SourceExpression = "OwnerName", Flag = 1 },
				new Responsibility() { Name = "Development", SourceExpression = "DeveloperName", Flag = 2 }
			}.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
		}
	}
}
using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System;
using System.Collections.Generic;
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
		/// Column expression that's valid within the AllWorkItems query that shows the assigned person.
		/// Not used, but my heart was in right place, and it might come back later.
		/// </summary>
		public string SourceExpression { get; set; }

		[DefaultExpression("0")]
		public int Flag { get; set; }

		/// <summary>
		/// Indicates what WorkItem property to set according to the Responsibility.Id in effect (which comes from the activity that was selected).
		/// The Id values are assumed from the order of the seed data records
		/// </summary>
		public static Dictionary<int, Action<WorkItem, int>> SetWorkItemUserActions
		{
			get
			{
				return new Dictionary<int, Action<WorkItem, int>>()
				{
					{ 1, (wi, userId) => wi.BusinessUserId = userId },
					{ 2, (wi, userId) => wi.DeveloperUserId = userId }
				};
			}
		}		

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
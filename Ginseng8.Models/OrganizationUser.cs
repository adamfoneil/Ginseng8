using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
	public class OrganizationUser : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[References(typeof(UserProfile))]
		[PrimaryKey]
		public int UserId { get; set; }

		[MaxLength(50)]
		public string DisplayName { get; set; }

		/// <summary>
		/// Responsibility.Flag sum
		/// </summary>
		[DefaultExpression("0")]
		public int Responsibilities { get; set; }

		/// <summary>
		/// WorkDay.Flag sum
		/// </summary>
		public int WorkDays { get; set; }

		/// <summary>
		/// Average daily productive work hours
		/// </summary>
		[DecimalPrecision(4,2)]
		public decimal DailyWorkHours { get; set; }

		/// <summary>
		/// Max number of work items considered in progress
		/// </summary>
		public int? MaxWorkInProgress { get; set; }

		/// <summary>
		/// This is a join request (or invite)
		/// </summary>
		public bool IsRequest { get; set; }

		/// <summary>
		/// User is allowed into the org (join request accepted)
		/// </summary>
		public bool IsEnabled { get; set; }

		public override bool Validate(IDbConnection connection, out string message)
		{
			message = null;
			if ((MaxWorkInProgress ?? 1) < 1)
			{								
				message = "Max WIP cannot be less than one.";
				return false;				
			}

			return true;
		}
	}
}
using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	public class OrganizationUser : BaseTable, IFindRelated<int>
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
		public int Responsibilities { get; set; } = 3;

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
		/// Application to filter for in Dashboard views
		/// </summary>
		[References(typeof(Application))]
		public int? CurrentAppId { get; set; }

		/// <summary>
		/// This is a join request (or invite)
		/// </summary>
		public bool IsRequest { get; set; }

		/// <summary>
		/// User is allowed into the org (join request accepted)
		/// </summary>
		public bool IsEnabled { get; set; }

		public Application CurrentApp { get; set; }

		[NotMapped]
		public string OrgName { get; set; }

		public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			if (CurrentAppId.HasValue)
			{
				CurrentApp = commandProvider.Find<Application>(connection, CurrentAppId.Value);
			}
		}

		public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			if (CurrentAppId.HasValue)
			{
				CurrentApp = await commandProvider.FindAsync<Application>(connection, CurrentAppId.Value);
			}
		}

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
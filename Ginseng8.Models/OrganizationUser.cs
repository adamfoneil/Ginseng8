using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	public class OrganizationUser : BaseTable, IFindRelated<int>, INotifyOptions, IOrgSpecific
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
		[DecimalPrecision(4, 2)]
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

        [References(typeof(Team))]
        public int? CurrentTeamId { get; set; }

		[References(typeof(Activity))]
		public int? DefaultActivityId { get; set; }

		[Column(TypeName = "money")]
		public decimal? InvoiceRate { get; set; }

		/// <summary>
		/// This is a join request (or invite)
		/// </summary>
		public bool IsRequest { get; set; }

		/// <summary>
		/// User is allowed into the org (join request accepted)
		/// </summary>
		public bool IsEnabled { get; set; }

		public Application CurrentApp { get; set; }

		public UserProfile UserProfile { get; set; }
		public Organization Organization { get; set; }

        [NotMapped]
        public decimal WeeklyHours { get; set; }

		[NotMapped]
		public string OrgName { get; set; }

		[NotMapped]
		public string UserName { get; set; }

		[NotMapped]
		public string Email { get; set; }

		[NotMapped]
		public string PhoneNumber { get; set; }

		[DefaultExpression("0")]
		public bool SendEmail { get; set; }

		[DefaultExpression("0")]
		public bool SendText { get; set; }

		[DefaultExpression("0")]
		public bool InApp { get; set; }

		[NotMapped]
		public string TableName => nameof(OrganizationUser);

		public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			if (CurrentAppId.HasValue)
			{
				CurrentApp = commandProvider.Find<Application>(connection, CurrentAppId.Value);
			}

			UserProfile = commandProvider.Find<UserProfile>(connection, UserId);
			Organization = commandProvider.Find<Organization>(connection, OrganizationId);
		}

		public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			if (CurrentAppId.HasValue)
			{
				CurrentApp = await commandProvider.FindAsync<Application>(connection, CurrentAppId.Value);
			}

			UserProfile = await commandProvider.FindAsync<UserProfile>(connection, UserId);
			Organization = await commandProvider.FindAsync<Organization>(connection, OrganizationId);
		}

		public static async Task<string> GetUserDisplayNameAsync(IDbConnection connection, int orgId, int userId, IUser user)
		{
			var orgUser = await connection.FindWhereAsync<OrganizationUser>(new { OrganizationId = orgId, UserId = userId });
			return (orgUser?.DisplayName != null) ? orgUser.DisplayName : user.UserName;
		}

        public static async Task<string> GetUserDisplayNameAsync(IDbConnection cn, int orgId, string userName)
        {
            var user = await cn.FindWhereAsync<UserProfile>(new { userName });
            var orgUser = await cn.FindWhereAsync<OrganizationUser>(new { OrganizationId = orgId, user.UserId });
            return (orgUser?.DisplayName != null) ? orgUser.DisplayName : userName;
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

		public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			// if this is an accepted join request, then it's no longer a request
			if (IsEnabled && IsRequest) IsRequest = false;
			base.BeforeSave(connection, action, user);
		}

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }
}
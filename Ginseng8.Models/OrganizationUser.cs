using Dapper;
using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
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

		[References(typeof(Activity))]
		public int? DefaultActivityId { get; set; }

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
		public string OrgName { get; set; }

		[NotMapped]
		public string UserName { get; set; }

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

		internal static async Task<string> GetUserDisplayNameAsync(IDbConnection connection, int orgId, int userId, IUser user)
		{
			var orgUser = await connection.FindWhereAsync<OrganizationUser>(new { OrganizationId = orgId, UserId = userId });
			return (orgUser?.DisplayName != null) ? orgUser.DisplayName : user.UserName;
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

		public static async Task ConnectPrincipalAsync(SqlConnection cn, ClaimsPrincipal principal, string orgName)
		{
			var properties = principal.Claims.OfType<Claim>().ToDictionary(item => item.Type, item => item.Value);

			string userName = properties["preferred_username"];

			// problem is user doesn't exist yet
			int userId = await cn.QuerySingleAsync<int>("SELECT [UserId] FROM [dbo].[AspNetUsers] WHERE [UserName]=@userName", new { userName });

			var profile = await cn.FindAsync<UserProfile>(userId);			
			int orgId = await cn.QuerySingleAsync<int>("SELECT [Id] FROM [dbo].[Organization] WHERE [name]=@orgName", new { orgName });
			var orgUser = await cn.FindWhereAsync<OrganizationUser>(new { OrganizationId = orgId, UserId = userId });

			var sysUser = new SystemUser() { UserName = "OAuth", LocalTime = DateTime.UtcNow };

			if (orgUser == null)
			{
				orgUser = new OrganizationUser()
				{
					UserId = userId,
					OrganizationId = orgId,
					DisplayName = properties["name"],
					DailyWorkHours = 6, // 8 is too many IMO
					WorkDays = 62, // mon -> fri
					IsEnabled = true
				};
				await cn.SaveAsync(orgUser, sysUser);
			}

			if (!profile.OrganizationId.HasValue)
			{
				profile.OrganizationId = orgId;
				await cn.UpdateAsync(profile, sysUser, r => r.OrganizationId);
			}
		}
	}
}
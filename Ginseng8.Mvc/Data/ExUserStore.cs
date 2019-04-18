using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Data
{
    public class ExUserStore : UserStore<IdentityUser>
    {		
		private readonly DataAccess _data;

        public ExUserStore(
            ApplicationDbContext context,
			IConfiguration config,
            IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
			_data = new DataAccess(config);
        }		

        /// <inheritdoc />
        public override async Task<IdentityResult> CreateAsync(
            IdentityUser user, 
            CancellationToken cancellationToken = new CancellationToken())
		{
			var result = await base.CreateAsync(user, cancellationToken);

			await CreateOrgUserAsync(user);

			return result;
		}

		/// <inheritdoc />
		public override async Task AddLoginAsync(
            IdentityUser user, 
            UserLoginInfo login,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await base.AddLoginAsync(user, login, cancellationToken);

			await CreateOrgUserAsync(user);
        }

		private async Task CreateOrgUserAsync(IdentityUser user)
		{
			using (var cn = _data.GetConnection())
			{
				await cn.ExecuteAsync(
					@"INSERT INTO [dbo].[OrganizationUser] (
						[OrganizationId], [UserId], [DisplayName], [DailyWorkHours], [Responsibilities], 
						[WorkDays], [IsEnabled], [IsRequest], [CreatedBy], [DateCreated]
					) SELECT
						[org].[Id], [u].[UserId], [er].[DisplayName], 6, 3, 62, 1, 0, 'OAuth', getutcdate()
					FROM
						[dbo].[ExternalRegistration] [er]
						INNER JOIN [dbo].[Organization] [org] ON [er].[TenantName]=[org].[TenantName]
						INNER JOIN [dbo].[AspNetUsers] [u] ON [er].[UserName]=[u].[UserName]
					WHERE
						[u].[UserName]=@userName AND
						NOT EXISTS(SELECT 1 FROM [dbo].[OrganizationUser] WHERE [OrganizationId]=[org].[Id] AND [UserId]=[u].[UserId])",
					new { userName = user.UserName });

				await cn.ExecuteAsync(
					@"UPDATE [u] SET
						[OrganizationId]=[org].[Id]
					FROM
						[dbo].[ExternalRegistration] [er]
						INNER JOIN [dbo].[Organization] [org] ON [er].[TenantName]=[org].[TenantName]
						INNER JOIN [dbo].[AspNetUsers] [u] ON [er].[UserName]=[u].[UserName]
					WHERE
						[u].[UserName]=@userName AND
						[u].[OrganizationId] IS NULL", new { userName = user.UserName });
			}
		}
	}
}

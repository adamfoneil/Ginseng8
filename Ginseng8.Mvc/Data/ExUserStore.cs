using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Ginseng.Mvc.Data
{
    public class ExUserStore : UserStore<IdentityUser>
    {
        public ExUserStore(
            ApplicationDbContext context,
            IdentityErrorDescriber describer = null)
            : base(context, describer)
        {

        }

        /// <inheritdoc />
        public override async Task<IdentityResult> CreateAsync(
            IdentityUser user, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.CreateAsync(user, cancellationToken);

            // Perform any business logic once a user entity is created

            return result;
        }

        /// <inheritdoc />
        public override async Task AddLoginAsync(
            IdentityUser user, 
            UserLoginInfo login,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await base.AddLoginAsync(user, login, cancellationToken);

            // Perform any business logic once a new login is added to an existed user entity
        }
    }
}

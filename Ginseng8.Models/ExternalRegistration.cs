using Postulate.Base.Attributes;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// Created when an OAuth/externa login is created. This enables creation of dbo.OrganizationUser in Mvc.ExUserStore
    /// </summary>
    [Identity(nameof(Id))]
    public class ExternalRegistration
    {
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        [PrimaryKey]
        public string UserName { get; set; }

        [MaxLength(50)]
        [Required]
        public string DisplayName { get; set; }

        [MaxLength(50)]
        [Required]
        public string TenantName { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public static async Task CreateAsync(SqlConnection cn, ClaimsPrincipal principal, string tenantName)
        {
            var claims = principal.Claims.OfType<Claim>().ToDictionary(item => item.Type, item => item.Value);
            string userName = claims["preferred_username"];

            if (await cn.ExistsWhereAsync<ExternalRegistration>(new { UserName = userName })) return;

            var externalReg = new ExternalRegistration()
            {
                UserName = userName,
                DisplayName = claims["name"],
                TenantName = tenantName
            };

            await cn.SaveAsync(externalReg);
        }
    }
}
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// Created by <see cref="OrganizationUser.ConnectPrincipalAsync(System.Data.SqlClient.SqlConnection, System.Security.Claims.ClaimsPrincipal, string)"/>
	/// Then an insert trigger on dbo.AspNetUsers generates dbo.OrganizationUser from this
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
	}
}
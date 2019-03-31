using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace Ginseng.Models
{
	public class Organization : BaseTable
	{
		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[References(typeof(UserProfile))]
		[ColumnAccess(SaveAction.Insert)]
		public int OwnerUserId { get; set; }

		[DefaultExpression("1000")]
		public int NextWorkItemNumber { get; set; } = 1000;

		public override bool Validate(IDbConnection connection, out string message)
		{
			if (Name.Contains("--"))
			{
				message = "Name may not contain consecutive dashes.";
				return false;
			}

			var allowedChars = "abcdefghijklmnopqrstuvwxyz1234567890-".ToCharArray();
			var nameChars = Name.Select(c => char.ToLower(c)).ToArray();
			var invalid = nameChars.Except(allowedChars);

			if (invalid.Any())
			{
				message = "Organization name may contain letters, numbers, and dashes only.";
				return false;
			}

			message = null;
			return true;
		}

		public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			base.BeforeSave(connection, action, user);

			if (action == SaveAction.Insert)
			{
				UserProfile profile = user as UserProfile;
				if (profile != null) OwnerUserId = profile.UserId;
			}
		}

		public override void AfterSave(IDbConnection connection, SaveAction action, IUser user)
		{
			base.AfterSave(connection, action, user);

			if (action == SaveAction.Insert)
			{
				var profile = connection.Find<UserProfile>(OwnerUserId);
				if (!profile.OrganizationId.HasValue)
				{
					profile.OrganizationId = Id;
					connection.Update(profile, null, r => r.OrganizationId);
				}
			}
		}
	}
}
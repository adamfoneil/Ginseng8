using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng8.Models.Conventions
{
	[Identity(nameof(Id), IdentityPosition.FirstColumn)]
	public abstract class BaseTable : Record
	{
		public int Id { get; set; }

		[MaxLength(50)]
		[Required]
		[ColumnAccess(SaveAction.Insert)]
		public string CreatedBy { get; set; }

		[ColumnAccess(SaveAction.Insert)]
		public DateTime DateCreated { get; set; }

		[MaxLength(50)]
		[ColumnAccess(SaveAction.Update)]
		public string ModifiedBy { get; set; }

		[ColumnAccess(SaveAction.Update)]
		public DateTime? DateModified { get; set; }

		public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			switch (action)
			{
				case SaveAction.Insert:
					CreatedBy = user.UserName;
					DateCreated = user.LocalTime;
					break;

				case SaveAction.Update:
					ModifiedBy = user.UserName;
					DateModified = user.LocalTime;
					break;
			}
		}
	}
}
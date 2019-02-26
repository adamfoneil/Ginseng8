using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng8.Models
{
	[Table("AspNetUsers")]
	[Identity(nameof(UserId), constraintName:"U_UserProfile_UserId")]
	public class UserProfile : IUser
	{		
		[MaxLength(256)]
		public string UserName { get; set; }

		[MaxLength(256)]
		public string Email { get; set; }

		public int UserId { get; set; }

		[DefaultExpression("0")]
		public int TimeZoneOffset { get; set; }

		public DateTime LocalTime
		{
			get
			{
				return (TimeZoneOffset < 24) ?
					DateTime.UtcNow.AddHours(TimeZoneOffset) :
					DateTime.UtcNow.AddMinutes(TimeZoneOffset);
			}
		}
	}
}
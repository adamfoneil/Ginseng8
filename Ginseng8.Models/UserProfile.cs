using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    [Table("AspNetUsers")]
    [Identity(nameof(UserId), constraintName: "U_UserProfile_UserId")]
    public class UserProfile : IUser
    {
        [MaxLength(256)]
        [UniqueKey]
        public string UserName { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }

        public int UserId { get; set; }

        [DefaultExpression("0")]
        public int TimeZoneOffset { get; set; }

        [DefaultExpression("1")]
        public bool AdjustForDaylightSaving { get; set; }

        [References(typeof(Organization))]
        public int? OrganizationId { get; set; }

        public DateTime LocalTime
        {
            get
            {
                int dst = GetDaylightSavingOffset();
                return (TimeZoneOffset < 24) ?
                    DateTime.UtcNow.AddHours(TimeZoneOffset + dst) :
                    DateTime.UtcNow.AddMinutes(TimeZoneOffset + dst);
            }
        }

        private int GetDaylightSavingOffset()
        {
            return (!DateTime.UtcNow.IsDaylightSavingTime() && AdjustForDaylightSaving) ? 1 : 0;
        }
    }
}
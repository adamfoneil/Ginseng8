using Dapper;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

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
            get { return GetLocalTime(DateTime.UtcNow, TimeZoneOffset, AdjustForDaylightSaving); }
        }

        public static DateTime GetLocalTime(DateTime dateTime, int offset, bool adjustDst)
        {
            int dst = GetDaylightSavingOffset(adjustDst);
            return (offset < 24) ?
                dateTime.AddHours(offset + dst) :
                dateTime.AddMinutes(offset + dst);
        }

        private static int GetDaylightSavingOffset(bool adjustDst)
        {
            return (!DateTime.UtcNow.IsDaylightSavingTime() && adjustDst) ? 1 : 0;
        }

        internal static async Task<int> GetUserIdAsync(IDbConnection connection, string userName)
        {
            return await connection.QuerySingleOrDefaultAsync<int>("SELECT [UserId] FROM [dbo].[AspNetUsers] WHERE [UserName]=@userName", new { userName });
        }
    }
}
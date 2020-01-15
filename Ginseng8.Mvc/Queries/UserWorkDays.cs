using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
    public class UserWorkDaysResult
    {
        public int UserId { get; set; }
        public int Value { get; set; }
        public string Abbreviation { get; set; }
        public decimal Hours { get; set; }
    }

    public class UserWorkDays : Query<UserWorkDaysResult>
    {
        public UserWorkDays() : base("SELECT * FROM [dbo].[FnUserWorkDays](@orgId) ORDER BY [Value]")
        {
        }

        public int OrgId { get; set; }
    }
}

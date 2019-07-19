using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    public class MilestoneUserView : BaseTable
    {        
        [PrimaryKey]
        public int MilestoneId { get; set; }

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}

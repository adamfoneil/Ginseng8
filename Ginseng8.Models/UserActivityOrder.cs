using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Lets Becky have her specific ordering of activities on My Items page
    /// </summary>
    public class UserActivityOrder : BaseTable
    {
        [PrimaryKey]
        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        [PrimaryKey]
        [References(typeof(Activity))]
        public int ActivityId { get; set; }

        public int Value { get; set; }
    }
}
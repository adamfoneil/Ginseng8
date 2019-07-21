using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    /// <summary>
    /// Associates a label and a team for use with Dashboard/New
    /// </summary>
    public class TeamLabel : BaseTable
    {
        [References(typeof(Team))]
        [PrimaryKey]
        public int TeamId { get; set; }

        [References(typeof(Label))]
        [PrimaryKey]
        public int LabelId { get; set; }

        [NotMapped]
        public string TeamName { get; set; }
    }
}
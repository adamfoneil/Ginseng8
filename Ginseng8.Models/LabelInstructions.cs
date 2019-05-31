using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Use this to add instructions for entering items on Dashboard/New
    /// </summary>
    public class LabelInstructions : BaseTable, IBody
    {
        [PrimaryKey]
        [References(typeof(Label))]
        public int LabelId { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }
    }
}
using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Associates a label with an app for use with Dashboard/New
    /// </summary>
    public class NewItemAppLabel : BaseTable
    {
        [References(typeof(Application))]
        [PrimaryKey]
        public int ApplicationId { get; set; }

        [References(typeof(Label))]
        [PrimaryKey]
        public int LabelId { get; set; }
    }
}
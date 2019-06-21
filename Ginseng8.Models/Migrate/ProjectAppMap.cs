using Postulate.Base.Attributes;

namespace Ginseng.Models.Migrate
{
    [Schema("migrate")]
    [Identity(nameof(Id))]
    public class ProjectAppMap
    {
        /// <summary>
        /// Source project Id
        /// </summary>
        [PrimaryKey]
        public int ProjectId { get; set; }

        /// <summary>
        /// Target app Id
        /// </summary>
        public int AppId { get; set; }

        public int Id { get; set; }
    }
}
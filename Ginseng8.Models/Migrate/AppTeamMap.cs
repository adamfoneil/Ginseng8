using Postulate.Base.Attributes;

namespace Ginseng.Models.Migrate
{
    /// <summary>
    /// Used to migrate work items from apps to their equivalent teams
    /// </summary>
    [Schema("migrate")]
    [Identity(nameof(Id))]
    public class AppTeamMap
    {
        /// <summary>
        /// Source applicationId
        /// </summary>
        [PrimaryKey]
        public int AppId { get; set; }

        /// <summary>
        /// Target teamId
        /// </summary>
        public int TeamId { get; set; }

        public int Id { get; set; }
    }
}
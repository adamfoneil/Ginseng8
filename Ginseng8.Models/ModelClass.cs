using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using Postulate.SqlServer.IntKey;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// Describes a database table
    /// </summary>
    [TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
    public class ModelClass : BaseTable, IBody, IOrgSpecific
    {
        [References(typeof(DataModel))]
        [PrimaryKey]
        public int DataModelId { get; set; }

        [MaxLength(50)]
        [PrimaryKey]
        public string Name { get; set; }

        [MaxLength(50)]
        public string ObjectName { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        /// <summary>
        /// If true, then class has no properties (used for "built-in" types like int, money, date, etc)
        /// </summary>
        public bool IsScalarType { get; set; }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            var dm = await connection.FindAsync<DataModel>(DataModelId);
            return await dm.GetOrgIdAsync(connection);
        }
    }
}
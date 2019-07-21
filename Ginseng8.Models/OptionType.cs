using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
    public enum OptionSourceType
    {
        None = 0,
        DelimitedStrings = 1,
        SqlQuery = 2,
        YesNo = 3
    }

    public class OptionType : AppTable
    {
        [PrimaryKey]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string StorageColumn { get; set; }        

        public OptionSourceType SourceType { get; set; }

        public static DataTable GetSeedData()
        {
            return new OptionType[]
            {
                new OptionType() { Name = "Plain Text", StorageColumn = "StringValue", SourceType = OptionSourceType.None },
                new OptionType() { Name = "Select Text (Delimited)", StorageColumn = "StringValue", SourceType = OptionSourceType.DelimitedStrings },
                new OptionType() { Name = "Select Text (Query)", StorageColumn = "StringValue", SourceType = OptionSourceType.SqlQuery },
                new OptionType() { Name = "Bool", StorageColumn = "BoolValue", SourceType = OptionSourceType.YesNo },
                new OptionType() { Name = "Int", StorageColumn = "IntValue", SourceType = OptionSourceType.None }
            }.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
        }
    }
}
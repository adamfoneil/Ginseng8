using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{    
    public class Option : AppTable
    {
        [PrimaryKey]
        [MaxLength(100)]
        public string Name { get; set; }

        [References(typeof(OptionType))]
        public int TypeId { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
        
        public string OptionSource { get; set; }

        public bool IsActive { get; set; } = true;

        public const string MyItemsFilterCurrentApp = "MyItems:FilterCurrentApp";
        public const string MyItemsGroupField = "MyItems:GroupField";
    }
}
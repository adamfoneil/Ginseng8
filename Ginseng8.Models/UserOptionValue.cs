using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class UserOptionValue : BaseTable, IFindRelated<int>
    {
        [References(typeof(Option))]
        [PrimaryKey]
        public int OptionId { get; set; }

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        [MaxLength(255)]
        public string StringValue { get; set; }

        public bool? BoolValue { get; set; }

        public int? IntValue { get; set; }

        [NotMapped]
        public string StorageColumn { get; set; }

        public Option Option { get; set; }
        public OptionType OptionType { get; set; }

        public object GetValue()
        {
            var props = GetType().GetProperties().ToDictionary(pi => pi.Name);
            return props[StorageColumn].GetValue(this);
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Option = commandProvider.Find<Option>(connection, OptionId);
            OptionType = commandProvider.Find<OptionType>(connection, Option.TypeId);
            StorageColumn = OptionType.StorageColumn;
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Option = await commandProvider.FindAsync<Option>(connection, OptionId);
            OptionType = await commandProvider.FindAsync<OptionType>(connection, Option.TypeId);
            StorageColumn = OptionType.StorageColumn;
        }
    }
}
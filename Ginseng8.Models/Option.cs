using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class Option : AppTable, IFindRelated<int>
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

        public OptionType OptionType { get; set; }

        public const string MyItemsFilterCurrentApp = "MyItems.FilterCurrentApp";
        public const string MyItemsGroupField = "MyItems.GroupField";
        public const string MyItemsUserIdField = "MyItems.UserIdField";

        public static async Task<Option> FindByName(SqlConnection connection, string name)
        {
            return await connection.FindWhereAsync<Option>(new { name });            
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            OptionType = commandProvider.Find<OptionType>(connection, TypeId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            OptionType = await commandProvider.FindAsync<OptionType>(connection, TypeId);
        }
    }
}
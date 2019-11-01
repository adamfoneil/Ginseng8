using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public class GithubIntegration : BaseTable, IFindRelated<int>
    {
        [PrimaryKey]
        [References(typeof(Application))]
        public int ApplicationId { get; set; }

        [PrimaryKey]
        public int RepositoryId { get; set; }

        [MaxLength(255)]
        public string RepositoryName { get; set; }

        public Application Application { get; set; }

        [NotMapped]
        public string ApplicationName { get; set; }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {            
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Application = await commandProvider.FindAsync<Application>(connection, ApplicationId);
        }
    }
}
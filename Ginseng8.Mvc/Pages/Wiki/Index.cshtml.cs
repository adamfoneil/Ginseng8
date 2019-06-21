using Ginseng.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Wiki
{
    [Authorize]
    public class IndexModel : WikiPageModel
    {
        public IndexModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Article Article { get; set; }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            if (Id != 0)
            {
                Article = await connection.FindAsync<Article>(Id);
            }
        }
    }
}
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class NewModel : DashboardPageModel
    {
        public NewModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<Label> Labels { get; set; }
        public IEnumerable<Application> Applications { get; set; }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            Applications = await new Applications() { OrgId = OrgId, IsActive = true, AllowNewItems = true }.ExecuteAsync(connection);
            Labels = await new Labels() { OrgId = OrgId, IsActive = true, AllowNewItems = true }.ExecuteAsync(connection);
        }

        protected override OpenWorkItems GetQuery()
        {
            return new OpenWorkItems()
            {
                HasProject = false
            };
        }
    }
}
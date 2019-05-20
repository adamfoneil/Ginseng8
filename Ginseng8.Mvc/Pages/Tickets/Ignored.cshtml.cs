using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Tickets
{
    public class IgnoredModel : AppPageModel
    {
        private readonly FreshdeskCache _cache;

        public IgnoredModel(
            IConfiguration config,
            IFreshdeskClientFactory freshdeskClientFactory) : base(config)
        {
            _cache = new FreshdeskCache(config, freshdeskClientFactory);
        }

        public string FreshdeskUrl { get; set; }

        public async Task OnGetAsync(int responsibilityId = 0)
        {
            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;

            await _cache.InitializeAsync(Data.CurrentOrg.Name);

            using (var cn = Data.GetConnection())
            {
            }
        }
    }
}
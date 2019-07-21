using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Tickets
{
    public class TicketPageModel : AppPageModel
    {
        public TicketPageModel(
            IConfiguration config,
            IFreshdeskClientFactory freshdeskClientFactory) : base(config)
        {
            FreshdeskCache = new FreshdeskCache(config, freshdeskClientFactory);
        }

        public int TeamId
        {
            get { return CurrentOrgUser.CurrentTeamId ?? 0; }
        }

        public Dictionary<string, string> TypeBadges
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Issue", "badge-danger" },
                    { "Feature Request", "badge-info" },
                    { "Request", "badge-warning" },
                    { "Bug", "badge-danger" }
                };
            }
        }

        public string GetTypeBadge(string type)
        {
            return (TypeBadges.ContainsKey(type)) ? TypeBadges[type] : "badge-light";
        }

        public string FreshdeskUrl { get; private set; }

        public IEnumerable<Ticket> Tickets { get; set; }
        public Dictionary<long, Group> Groups { get { return FreshdeskCache.GroupDictionary; } }

        protected FreshdeskCache FreshdeskCache { get; }
        protected IEnumerable<long> IgnoredTickets { get; private set; }

        public string GetContactName(long requesterId)
        {
            return (FreshdeskCache.ContactDictionary.ContainsKey(requesterId)) ? FreshdeskCache.ContactDictionary[requesterId].Name : $"requester id {requesterId}";
        }

        public string GetCompanyName(long companyId)
        {
            return (FreshdeskCache.CompanyDictionary.ContainsKey(companyId)) ? FreshdeskCache.CompanyDictionary[companyId].Name : $"company id {companyId}";
        }

        /// <summary>
        /// This is virtual because you might want ignored tickets paginated,
        /// see <see cref="IgnoredModel"/>
        /// </summary>
        protected virtual async Task<IEnumerable<long>> GetIgnoredTicketsAsync(IDbConnection connection)
        {
            return await new IgnoredTickets() { TeamId = CurrentOrgUser.CurrentTeamId ?? 0, OrgId = OrgId }.ExecuteAsync(connection);
        }

        protected async Task InitializeAsync()
        {
            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;
            await FreshdeskCache.InitializeAsync(Data.CurrentOrg.Name);

            using (var cn = Data.GetConnection())
            {
                IgnoredTickets = await GetIgnoredTicketsAsync(cn);
            }
        }
    }
}
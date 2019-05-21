using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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

        public Dictionary<string, string> TypeBadges
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Issue", "badge-danger" },
                    { "Feature Request", "badge-info" },
                    { "Request", "badge-warning" }
                };
            }
        }

        public string GetTypeBadge(string type)
        {
            return (TypeBadges.ContainsKey(type)) ? TypeBadges[type] : "badge-light";
        }

        public string FreshdeskUrl { get; private set; }

        [BindProperty(SupportsGet = true)]
        public int ResponsibilityId { get; set; }

        public SelectList ResponsibilitySelect { get; set; }
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

        protected async Task InitializeAsync(int responsibilityId)
        {
            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;
            await FreshdeskCache.InitializeAsync(Data.CurrentOrg.Name);

            using (var cn = Data.GetConnection())
            {
                ResponsibilitySelect = await new ResponsibilitySelect().ExecuteSelectListAsync(cn, responsibilityId);
                IgnoredTickets = await new IgnoredTickets() { ResponsibilityId = responsibilityId, OrgId = OrgId }.ExecuteAsync(cn);                
            }
        }
    }
}
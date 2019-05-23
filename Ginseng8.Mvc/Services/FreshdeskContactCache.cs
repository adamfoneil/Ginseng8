using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
    public class FreshdeskContactCache : IntegrationCache<Contact>
    {
        private readonly IFreshdeskClientFactory _clientFactory;

        public FreshdeskContactCache(
            IConfiguration config,
            IFreshdeskClientFactory factory) : base(config, "Contacts")
        {
            _clientFactory = factory;
        }

        public override TimeSpan CallInterval => TimeSpan.FromMinutes(15);

        protected override async Task<IEnumerable<Contact>> ApiQueryAll(string orgName)
        {
            var client = await _clientFactory.CreateClientForOrganizationAsync(orgName);
            var results = await client.ListContactsAsync();
            return results;
        }

        protected override async Task<Contact> ConvertFromBlobAsync(CloudBlockBlob blob)
        {
            string json = await blob.DownloadTextAsync();
            return JsonConvert.DeserializeObject<Contact>(json);
        }

        protected override string GetBlobName(Contact @object)
        {
            return $"{@object.Id}.json";
        }
    }
}
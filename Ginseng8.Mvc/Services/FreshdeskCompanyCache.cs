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
    public class FreshdeskCompanyCache : IntegrationCache<Company>
    {
        private readonly IFreshdeskClientFactory _clientFactory;

        public FreshdeskCompanyCache(
            IConfiguration config,
            IFreshdeskClientFactory factory) : base(config, "Companies")
        {
            _clientFactory = factory;
        }

        public override TimeSpan CallInterval => TimeSpan.FromMinutes(15);

        protected override async Task<IEnumerable<Company>> ApiQueryAll(string orgName)
        {
            var client = await _clientFactory.CreateClientForOrganizationAsync(orgName);
            var results = await client.ListCompaniesAsync();
            return results;
        }

        protected override async Task<Company> ConvertFromBlobAsync(CloudBlockBlob blob)
        {
            string json = await blob.DownloadTextAsync();
            return JsonConvert.DeserializeObject<Company>(json);
        }

        protected override string GetBlobName(Company @object)
        {
            return $"{@object.Id}.json";
        }
    }
}
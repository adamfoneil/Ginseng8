using System;
using System.Threading.Tasks;
using Ginseng.Integration.Services;
using Ginseng.Models;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;

namespace Ginseng.Mvc.Services
{
    public class FreshdeskClientFactory : IFreshdeskClientFactory
    {
        private readonly DataAccess _data;        

        public FreshdeskClientFactory(IConfiguration config)
        {
            _data = new DataAccess(config);
        }

        /// <inheritdoc />
        public IFreshdeskClient CreateClient(string url, string key)
        {
            if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
            if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));

            return new FreshdeskClient(_data, url, key);
        } 

        /// <inheritdoc />
        public IFreshdeskClient CreateClientForOrganization(Organization organization)
        {
            if (organization.FreshdeskUrl.IsNullOrEmpty() || organization.FreshdeskApiKey.IsNullOrEmpty())
                throw new ArgumentException("Organization configuration is incorrect, FreshdeskUrl and FreshdeskApiKey fields must be specified");

            return CreateClient(organization.FreshdeskUrl, organization.FreshdeskApiKey);
        }

        /// <inheritdoc />
        public async Task<IFreshdeskClient> CreateClientForOrganizationAsync(int organizationId)
        {
            using (var cn = _data.GetConnection())
            {
                var organization = await cn.FindAsync<Organization>(organizationId);
                return CreateClientForOrganization(organization);
            }
        }

        /// <inheritdoc />
        public async Task<IFreshdeskClient> CreateClientForOrganizationAsync(string organizationName)
        {
            using (var cn = _data.GetConnection())
            {
                var organization = await cn.FindWhereAsync<Organization>(new { name = organizationName });
                return CreateClientForOrganization(organization);
            }
        }
    }
}

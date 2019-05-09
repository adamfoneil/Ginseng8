using Ginseng.Models;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Interfaces
{
    /// <summary>
    /// Freshdesk client factory
    /// </summary>
    public interface IFreshdeskClientFactory
    {
        /// <summary>
        /// Creates a Freshdesk client instance
        /// </summary>
        /// <param name="url">Freshdesk URL</param>
        /// <param name="key">Freshdesk authentication key</param>
        /// <returns>Freshdesk client instance</returns>
        IFreshdeskClient CreateClient(string url, string key);

        /// <summary>
        /// Creates a Freshdesk client instance for the specified organization
        /// </summary>
        /// <param name="organization">Organization</param>
        /// <returns>Freshdesk client instance</returns>
        IFreshdeskClient CreateClientForOrganization(Organization organization);

        /// <summary>
        /// Creates a Freshdesk client instance for the specified organization id
        /// </summary>
        /// <param name="organizationId">Organization id</param>
        /// <returns>Freshdesk client instance</returns>
        Task<IFreshdeskClient> CreateClientForOrganizationAsync(int organizationId);

        /// <summary>
        /// Creates a Freshdesk client instance for the specified organization name
        /// </summary>
        /// <param name="organizationName">Organization name</param>
        /// <returns>Freshdesk client instance</returns>
        Task<IFreshdeskClient> CreateClientForOrganizationAsync(string organizationName);
    }
}

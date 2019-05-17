using Ginseng.Models;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Services;
using Newtonsoft.Json;
using Postulate.SqlServer.IntKey;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Integration.Services
{
    public class FreshdeskClient : IFreshdeskClient
    {
        private readonly string _hostUrl;
        private readonly string _apiKey;
        private readonly string _endpointPrefix;
        private readonly DataAccess _data;
        private readonly int _orgId;

        public FreshdeskClient(DataAccess dataAccess, int orgId, string hostUrl, string apiKey, string endpointPrefix = "/api/v2")
        {
            _hostUrl = hostUrl;
            _apiKey = apiKey;
            _endpointPrefix = endpointPrefix;
            _data = dataAccess;
            _orgId = orgId;
        }

        public async Task<IEnumerable<Group>> ListGroupsAsync() 
            => await ExecuteAsync<List<Group>>("/groups", Method.GET);
        
        /// <inheritdoc />
        public async Task<IEnumerable<Ticket>> ListTicketsAsync()
            => await ExecuteAsync<List<Ticket>>("/tickets", Method.GET);

        public Task AddNoteAsync(long id, Comment comment, string userName)
            => ExecuteAsync<dynamic>($"/tickets/{id}/notes", Method.POST, new AddNoteRequest(comment, userName).ToJson());

        /// <inheritdoc />
        public Task<Ticket> GetTicketAsync(long id)
            => ExecuteAsync<Ticket>($"/tickets/{id}", Method.GET);

        /// <inheritdoc />
        public Task UpdateTicketWorkItemAsync(long id, string value)
            => ExecuteAsync<Ticket>($"/tickets/{id}", Method.PUT, new UpdateTicketWorkItemRequest(value).ToJson());

        /// <summary>
        /// This affects our daily usage limit, so be aware
        /// </summary>
        private async Task<T> ExecuteAsync<T>(string resource, Method method, string content = null)
        {
            var client = GetClient();
            var request = GetRequest(resource, method);

            if (!content.IsNullOrEmpty())
            {
                request.AddParameter("application/json", content, ParameterType.RequestBody);
            }

            var log = await LogAPICallAsync(method, resource, content);

            var response = await client.ExecuteTaskAsync<T>(request);            

            await CompleteLogEntryAsync(log, response);

            if (!response.IsSuccessful) throw new Exception(response.StatusDescription);

            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception exc)
            {
                // response.Content should be logged somewhere maybe
                throw new Exception($"API call to {resource} succeeded, but Json deserialization of the result failed. {exc.Message}", exc);
            }
        }

        private async Task CompleteLogEntryAsync<T>(APICall log, IRestResponse<T> response)
        {
            log.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful) log.StatusDescription = response.StatusDescription;
            using (var cn = _data.GetConnection())
            {
                await cn.SaveAsync(log);
            }
        }

        private async Task<APICall> LogAPICallAsync(Method method, string resource, string content)
        {
            var log = new APICall()
            {
                OrganizationId = _orgId,
                BaseUrl = _hostUrl,
                Method = method.ToString(),
                Resource = resource,
                Content = content
            };

            using (var cn = _data.GetConnection())
            {
                await cn.InsertAsync(log);
            }

            return log;
        }

        private RestClient GetClient()
        {
            var client = new RestClient(_hostUrl)
            {
                Authenticator = new HttpBasicAuthenticator(_apiKey, "X")
            };

            return client;
        }

        private RestRequest GetRequest(string resource, Method method)
        {
            return new RestRequest(_endpointPrefix + resource, method);
        }
    }
}
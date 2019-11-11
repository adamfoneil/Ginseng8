using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages
{
    public class ChartsModel : AppPageModel
    {
        public ChartsModel(IConfiguration config) : base(config)
        {
        }

        public string SqlChartifyToken { get; set; }
        public string SqlChartifyHost { get; set; }

        public async Task OnGetAsync()
        {
            SqlChartifyHost = Config["SqlChartify:HostUrl"];
            SqlChartifyToken = await SqlChartifyLoginAsync();
        }

        private async Task<string> SqlChartifyLoginAsync()
        {
            var login = new
            {                
                userName = Config["SqlChartify:UserName"],
                password = Config["SqlChartify:Password"]
            };

            var client = new RestClient(SqlChartifyHost);
            var request = new RestRequest("api/account/token");
            request.AddParameter("application/json", JsonConvert.SerializeObject(login), ParameterType.RequestBody);
            var response = await client.ExecutePostTaskAsync(request);
            if (response.IsSuccessful) return response.Content;

            throw new Exception($"SqlChartify login failed: {response.ErrorMessage}");
        }
    }
}
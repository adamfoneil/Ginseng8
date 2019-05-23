using AutoMapper;
using Ginseng.Mvc.Models.Freshdesk;
using Ginseng.Mvc.Models.Freshdesk.Dto;

namespace Ginseng.Mvc.Mapping
{
    /// <summary>
    /// Freshdesk service and controller mapping profile
    /// </summary>
    public class FreshdeskMappingProfile : Profile
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FreshdeskMappingProfile()
        {
            CreateMap<WebhookRequest, Webhook>().ConvertUsing<WebhookRequestToWebhookConverter>();
        }
    }
}

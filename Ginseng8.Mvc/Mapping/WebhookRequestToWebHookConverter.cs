using System;
using System.Collections.Generic;
using AutoMapper;
using Ginseng.Models.Enums.Freshdesk;
using Ginseng.Mvc.Enums.Freshdesk;
using Ginseng.Mvc.Models.Freshdesk;
using Ginseng.Mvc.Models.Freshdesk.Dto;

namespace Ginseng.Mvc.Mapping
{
    /// <summary>
    /// Convertor: Webhook DTO request to Webhook business layer model
    /// </summary>
    public class WebhookRequestToWebhookConverter : ITypeConverter<WebhookRequest, Webhook>
    {
        /// <summary>
        /// Converts Webhook DTO request to Webhook business layer model
        /// </summary>
        /// <param name="request">Webhook DTO request</param>
        /// <param name="destination">Webhook business layer model</param>
        /// <param name="context">Mapping profile</param>
        /// <returns>Webhook business layer model</returns>
        public Webhook Convert(WebhookRequest request, Webhook destination, ResolutionContext context)
        {
            var webhook = new Webhook()
            {
                Received = DateTimeOffset.UtcNow
            };

            switch (request.Event)
            {
                case "{ticket_action:update}":
                    webhook.Event = WebhookEvent.TicketUpdated;
                    break;

                case "{ticket_action:delete}":
                    webhook.Event = WebhookEvent.TicketDeleted;
                    break;

                default:
                    webhook.Event = WebhookEvent.Unknown;
                    break;
            }

            if (request.Ticket == null) return webhook;

            webhook.Ticket = new TicketInfo()
            {
                Id = request.Ticket.Id,
                Subject = request.Ticket.Subject,
                Url = request.Ticket.Url,
                Type = TicketTypeFromString(request.Ticket.Type),
                Status = TicketStatusFromString(request.Ticket.Status)
            };
            
            if (webhook.Event == WebhookEvent.TicketDeleted)
            {
                webhook.Ticket.Status = TicketStatus.Deleted;
            }

            return webhook;
        }

        public static TicketStatus TicketStatusFromString(string status)
        {
            var map = new Dictionary<string, TicketStatus>()
            {
                { "New", TicketStatus.New },
                { "Open", TicketStatus.Open },
                { "Pending", TicketStatus.Pending },
                { "Resolved", TicketStatus.Resolved },
                { "Closed", TicketStatus.Closed },
                { "Waiting on Customer", TicketStatus.WaitingOnCustomer },
                { "Waiting on Third Party", TicketStatus.WaitingOnThirdParty }
            };

            try
            {
                return map[status];
            }
            catch 
            {
                return TicketStatus.Unknown;
            }            
        }

        public static TicketType TicketTypeFromString(string type)
        {
            var map = new Dictionary<string, TicketType>()
            {
                { "Question", TicketType.Question },
                { "Incident", TicketType.Incident },
                { "Problem", TicketType.Problem },
                { "Feature source", TicketType.FeatureRequest },                
            };

            try
            {
                return map[type];
            }
            catch 
            {
                return TicketType.None;
            }
        }
    }
}

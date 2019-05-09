using System;
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
                Url = request.Ticket.Url
            };

            switch (request.Ticket.Type)
            {
                case "Question":
                    webhook.Ticket.Type = TicketType.Question;
                    break;

                case "Incident":
                    webhook.Ticket.Type = TicketType.Incident;
                    break;

                case "Problem":
                    webhook.Ticket.Type = TicketType.Problem;
                    break;

                case "Feature source":
                    webhook.Ticket.Type = TicketType.FeatureRequest;
                    break;

                default:
                    webhook.Ticket.Type = TicketType.None;
                    break;
            }

            switch (request.Ticket.Status)
            {
                case "New":
                    webhook.Ticket.Status = TicketStatus.New;
                    break;

                case "Open":
                    webhook.Ticket.Status = TicketStatus.Open;
                    break;

                case "Pending":
                    webhook.Ticket.Status = TicketStatus.Pending;
                    break;

                case "Resolved":
                    webhook.Ticket.Status = TicketStatus.Resolved;
                    break;

                case "Closed":
                    webhook.Ticket.Status = TicketStatus.Closed;
                    break;

                case "Waiting on Customer":
                    webhook.Ticket.Status = TicketStatus.WaitingOnCustomer;
                    break;

                case "Waiting on Third Party":
                    webhook.Ticket.Status = TicketStatus.WaitingOnThirdParty;
                    break;

                default:
                    webhook.Ticket.Status = TicketStatus.Unknown;
                    break;
            }

            if (webhook.Event == WebhookEvent.TicketDeleted)
            {
                webhook.Ticket.Status = TicketStatus.Deleted;
            }

            return webhook;
        }
    }
}

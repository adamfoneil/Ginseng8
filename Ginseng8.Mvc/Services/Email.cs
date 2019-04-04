using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
	public class Email
	{
		private readonly string _apiKey;
		private readonly string _senderEmail;
		private readonly string _senderName;

		public Email(IConfiguration config)
		{
			var section = config.GetSection("SendGrid");
			_apiKey = section.GetValue<string>("ApiKey");
			_senderName = section.GetValue<string>("SenderName");
			_senderEmail = section.GetValue<string>("SenderEmail");
		}

		public async Task<Response> SendAsync(string to, string subject, string textContent)
		{
			return await SendAsync(to, subject, textContent, textContent);
		}

		public async Task<Response> SendAsync(string to, string subject, string textContent, string htmlContent)
		{
			var client = new SendGridClient(_apiKey);
			var from = new EmailAddress(_senderEmail, _senderName);

			var sendTo = new EmailAddress(to);
			var message = MailHelper.CreateSingleEmail(from, sendTo, subject, textContent, htmlContent);

			return await client.SendEmailAsync(message);
		}
	}
}
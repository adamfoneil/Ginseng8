using Ginseng.Models;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Controllers
{
	public class NotificationController : Controller
	{
		private readonly DataAccess _data;
		private readonly string _validKey;
		private readonly Email _email;		

		public NotificationController(IConfiguration config)
		{
			_data = new DataAccess(config);
			_validKey = config.GetValue<string>("Notification:Key");
			_email = new Email(config);			
		}

		private bool IsValidKey(string key)
		{
			return (key?.Equals(_validKey) ?? false);
		}

		private async Task<IActionResult> OnValidKeyAsync(string key, Func<Task> action)
		{
			if (IsValidKey(key))
			{
				await action.Invoke();
				return Ok();
			}

			return BadRequest();
		}

		/// <summary>
		/// Set this up to be called by cron-job.org every 10 minutes or some other acceptable interval
		/// </summary>				
		public async Task<IActionResult> SendEmail([FromQuery]string key, int batchSize = 50)
		{
			return await OnValidKeyAsync(key, async () =>
			{
				using (var cn = _data.GetConnection())
				{
					var emails = await new PendingNotifications(batchSize) { Method = DeliveryMethod.Email }.ExecuteAsync(cn);
					foreach (var msg in emails)
					{
						string content = await this.RenderViewAsync("Notification", msg);
						await _email.SendAsync(msg.SendTo, $"{msg.ApplicationName} {msg.EventName} - {msg.WorkItemNumber}", content);
						await msg.MarkDeliveredAsync(cn);
					}
				}
			});
		}		
	}
}
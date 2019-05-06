using Dapper;
using Ginseng.Models;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Data.SqlClient;
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

		[AllowAnonymous]
        public async Task<IActionResult> Unsubscribe(int id)
		{
			using (var cn = _data.GetConnection())
			{
				var notif = await cn.FindAsync<Notification>(id);								

				switch (notif.Method)
				{
					case DeliveryMethod.Email:
						await UnsubscribeEmailAsync(cn, notif);
						break;

					default:
						// not implemented
						break;
				}
								
				return View();
			}
		}

		private async Task UnsubscribeEmailAsync(SqlConnection cn, Notification notif)
		{
			var user = await cn.FindWhereAsync<UserProfile>(new { UserName = notif.SendTo });
			if (user == null) return;

			string command = null;
			DynamicParameters dp = new DynamicParameters();

			switch (notif.SourceTable)
			{
				case nameof(Comment):
					var orgUser = cn.FindWhere<OrganizationUser>(new { notif.EventLog.OrganizationId, user.UserId });
					dp.Add("Id", orgUser.Id);
					command = "UPDATE [dbo].[OrganizationUser] SET [SendEmail]=0 WHERE [Id]=@id";					 
					break;

				case nameof(EventSubscription):
					command = "UPDATE [dbo].[EventSubscrption] SET [SentEmail]=0 WHERE [Id]=@id";
					dp.Add("Id", notif.SourceId);
					break;

				case nameof(ActivitySubscription):
					command = "UPDATE [dbo].[ActivitySubscription] SET [SendEmail]=0 WHERE [Id]=@id";
					dp.Add("Id", notif.SourceId);
					break;
			}

			await cn.ExecuteAsync(command, dp);
		}
	}
}
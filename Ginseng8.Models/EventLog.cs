using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	[Identity(nameof(Id), IdentityPosition.FirstColumn)]
	public class EventLog
	{
		/// <summary>
		/// A default constructor needed so that Dapper queries still work
		/// </summary>
		public EventLog()
		{
		}

		/// <summary>
		/// Use this constructor to ensure that App and Org Id are set during <see cref="WriteAsync(IDbConnection, EventLog)"/>
		/// </summary>		
		public EventLog(int workItemId, IUser user)
		{
			WorkItemId = workItemId;
			DateCreated = user.LocalTime;
			CreatedBy = user.UserName;
		}

		public int Id { get; set; }

		[References(typeof(Event))]
		public SystemEvent EventId { get; set; }

		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		[References(typeof(Application))]
		public int ApplicationId { get; set; }

		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		[MaxLength(50)]
		public string IconClass { get; set; }

		[MaxLength(50)]
		public string IconColor { get; set; }

		/// <summary>
		/// Used with email and Dashboard/Feed display
		/// </summary>
		[Required]
		public string HtmlBody { get; set; }

		/// <summary>
		/// Used with text messages (plain text, no markdown)
		/// </summary>
		[Required]
		public string TextBody { get; set; }

		[MaxLength(50)]
		public string CreatedBy { get; set; }

		public DateTime DateCreated { get; set; }

		public static async Task WriteAsync(IDbConnection connection, EventLog eventLog, IUser user)
		{
			eventLog.CreatedBy = user.UserName;
			eventLog.DateCreated = user.LocalTime;
			await WriteAsync(connection, eventLog);
		}

		public static async Task WriteAsync(IDbConnection connection, EventLog eventLog)
		{
			// the app and org might not be readily available in the various model classes that trigger events,
			// so I provide a little helper for getting those during log write
			if (eventLog.OrganizationId == 0 || eventLog.ApplicationId == 0)
			{
				var orgAndApp = await WorkItem.GetOrgAndAppIdAsync(connection, eventLog.WorkItemId);
				eventLog.OrganizationId = orgAndApp.OrganizationId;
				eventLog.ApplicationId = orgAndApp.ApplicationId;
			}

			await connection.PlainInsertAsync(eventLog);

			await Notification.CreateFromEventSubscriptions(connection, eventLog.Id);
		}				
	}
}
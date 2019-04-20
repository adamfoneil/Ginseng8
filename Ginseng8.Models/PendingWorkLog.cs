using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	public enum HoursSourceType
	{
		Comment = 1,
		CommitMessage = 2
	}

	public class PendingWorkLog : BaseTable, IBody
	{
		/// <summary>
		/// Work must at minimum be related to a project
		/// </summary>
		[References(typeof(Project))]
		public int ProjectId { get; set; }

		/// <summary>
		/// work hours are usually in reference to a specific work item.
		/// If there's no work item, it means the work relates to project definition/management by itself
		/// </summary>
		[References(typeof(WorkItem))]
		public int? WorkItemId { get; set; }		

		[References(typeof(UserProfile))]
		public int UserId { get; set; }

		[Column(TypeName = "date")]
		public DateTime Date { get; set; }

		[DecimalPrecision(4,2)]
		public decimal Hours { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		/// <summary>
		/// if the hours came from a comment or commit message, that's indicated here
		/// </summary>
		public HoursSourceType? SourceType { get; set; }

		/// <summary>
		/// Commit message or comment id that this record was generated from
		/// </summary>
		public int? SourceId { get; set; }

		public static async Task FromCommentAsync(IDbConnection connection, Comment comment, UserProfile user)
		{
			(int projectId, int? workItemId) = await FindProjectAndWorkItemIdAsync(connection, comment);
		
			var workLog = new PendingWorkLog()
			{
				ProjectId = projectId,
				WorkItemId = workItemId,
				UserId = user.UserId,
				Date = comment.DateCreated,
				Hours = ParseHoursFromBody(comment.TextBody),
				TextBody = comment.TextBody,
				HtmlBody = comment.HtmlBody,
				SourceType = HoursSourceType.Comment,
				SourceId = comment.Id
			};

			await connection.SaveAsync(workLog, user);
		}

		public static decimal ParseHoursFromBody(string input)
		{
			var match = Regex.Match(input, @"\+(\d*(\.[0-9][0-9]?)?)");
			throw new NotImplementedException();
		}

		private static Task<(int projectId, int? workItemId)> FindProjectAndWorkItemIdAsync(IDbConnection connection, Comment comment)
		{
			throw new NotImplementedException();
		}
	}
}
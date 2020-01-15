using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;
using System.Collections.Generic;
using Dapper.QX.Models;

namespace Ginseng.Mvc.Queries
{
	public class Attachments : Query<Attachment>
	{
		private readonly List<QueryTrace> _traces;

		public Attachments(List<QueryTrace> traces = null) : base(
			@"SELECT [a].*,
			CASE 
				WHEN @userName = [a].[CreatedBy] THEN 1
				ELSE 0
			END AS [AllowDelete]
			FROM [dbo].[Attachment] [a]
			WHERE [OrganizationId]=@orgId {andWhere}")
		{
			_traces = traces;
		}

		public int OrgId { get; set; }
		public string UserName { get; set; }

		[Where("[a].[ObjectType]=@objectType")]
		public ObjectType? ObjectType { get; set; }

		[Where("[a].[ObjectId]=@objectId")]
		public int? ObjectId { get; set; }

		protected override void OnQueryExecuted(QueryTrace queryTrace)
		{
			_traces?.Add(queryTrace);			
		}
	}
}
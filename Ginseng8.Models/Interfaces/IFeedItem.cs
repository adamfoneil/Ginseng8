using System;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models.Interfaces
{
	public interface IFeedItem
	{
		Task SetOrgAndAppIdAsync(IDbConnection connection);
		int OrganizationId { get; set; }
		int ApplicationId { get; set; }
		int WorkItemId { get; set; }
		string IconClass { get; }
		string HtmlBody { get; }
		string CreatedBy { get; }
		DateTime DateCreated { get; }
	}
}
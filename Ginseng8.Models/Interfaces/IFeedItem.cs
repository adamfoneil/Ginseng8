using System;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models.Interfaces
{
	public interface IFeedItem
	{
		Task SetOrganizationIdAsync(IDbConnection connection);
		int OrganizationId { get; set; }
		int WorkItemId { get; set; }
		string IconClass { get; }
		string HtmlBody { get; }
		string CreatedBy { get; }
		DateTime DateCreated { get; }
	}
}
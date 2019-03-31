using System;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models.Interfaces
{
	/// <summary>
	/// Used for event logging across the system
	/// </summary>
	public interface IFeedItem
	{
		/// <summary>
		/// On model classes where we use IFeedItem, the Org and App Id might not be properties of the model (because they would break normalization).
		/// So, there needed to be a way to get these when an event is being logged, so that's what this method is for.
		/// I considered some fancy SQL footwork inside the logging method, but this turned out to be simpler.
		/// </summary>			
		Task SetOrgAndAppIdAsync(IDbConnection connection);
		SystemEvent EventId { get; }
		int OrganizationId { get; }
		int ApplicationId { get; }
		int WorkItemId { get; set; }
		string IconClass { get; }
		string HtmlBody { get; }
		string CreatedBy { get; }
		DateTime DateCreated { get; }
	}
}
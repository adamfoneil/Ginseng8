using System;

namespace Ginseng.Models.Interfaces
{
	public interface IStatusUpdate
	{
		int WorkItemId { get; set; }
		string IconClass { get; }
		string HtmlBody { get; }
		string CreatedBy { get; }
		DateTime DateCreated { get; }
	}
}
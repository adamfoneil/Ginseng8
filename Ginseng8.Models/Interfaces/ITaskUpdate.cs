using System;

namespace Ginseng.Models.Interfaces
{
	public interface ITaskUpdate
	{
		int TaskId { get; set; }
		string IconClass { get; }
		string HtmlBody { get; }
		string CreatedBy { get; }
		DateTime DateCreated { get; }
	}
}
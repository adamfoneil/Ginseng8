using System;

namespace Ginseng.Mvc.Interfaces
{
	/// <summary>
	/// Enables consistent access to user and org info in views without querying it more than once
	/// </summary>
	internal interface IUserInfo
	{
		int UserId { get; }
		int OrgId { get; }
		DateTime LocalTime { get; }
	}
}
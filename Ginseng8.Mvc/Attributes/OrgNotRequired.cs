using System;

namespace Ginseng.Mvc.Attributes
{
	/// <summary>
	/// Use this to prevent checking value of UserProfile.OrganizationId in a controller or action
	/// See <see cref="AppPageModel.OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext)"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class OrgNotRequired : Attribute
	{
	}
}
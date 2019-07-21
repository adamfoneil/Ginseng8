using Microsoft.AspNetCore.Http;
using System;

namespace Ginseng.Mvc.Helpers
{
    public static partial class HtmlHelpers
    {
        /// <summary>
        /// if the current URL has route values, returns defaultUrl otherwise the current url.
        /// This is intended to prevent errors related to org switching
        /// </summary>
        public static string OrgSwitchReturnUrl(this HttpRequest request, string defaultUrl)
        {
            throw new NotImplementedException();
        }
    }
}
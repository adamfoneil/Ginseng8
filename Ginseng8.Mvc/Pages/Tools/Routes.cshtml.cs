using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Pages.Tools
{
    /// <summary>
    /// Based on https://github.com/ardalis/AspNetCoreRouteDebugger/blob/master/SampleProject/Controllers/RoutesController.cs
    /// </summary>
    [Authorize]
    public class RoutesModel : PageModel
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public RoutesModel(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public IEnumerable<RouteInfo> Routes { get; set; }

        public void OnGet()
        {
            Routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(x => new RouteInfo
            {
                Action = x.RouteValues["Action"],
                Controller = x.RouteValues["Controller"],
                RouteName = x.AttributeRouteInfo?.Name,
                Template = x.AttributeRouteInfo?.Template,
                Constraints = x.ActionConstraints ?? Enumerable.Empty<IActionConstraintMetadata>()
            }).ToList();
        }
    }

    public class RouteInfo
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string RouteName { get; set; }
        public string Template { get; set; }
        public IEnumerable<IActionConstraintMetadata> Constraints { get; set; }
    }
}
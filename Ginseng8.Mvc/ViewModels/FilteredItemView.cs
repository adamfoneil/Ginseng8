using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
    public class FilteredItemView : IUserInfo
    {
        public FilteredItemView(IUserInfo userInfo)
        {
            UserId = userInfo.UserId;
            OrgId = userInfo.OrgId;
            LocalTime = userInfo.LocalTime;
        }

        public DashboardPageModel Page { get; set; }
        public IEnumerable<OpenWorkItemsResult> WorkItems { get; set; }
        public ILookup<int, Comment> Comments { get; set; }
        public IEnumerable<SelectListItem> AssignToUsers { get; set; }

        public int UserId { get; set; }
        public int OrgId { get; set; }
        public DateTime LocalTime { get; set; }
    }
}
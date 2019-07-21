﻿using Ginseng.Models;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
    public class HandOffModel : AppPageModel
    {
        public HandOffModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public OpenWorkItemsResult WorkItem { get; set; }
        public HandOff HandOff { get; set; }
        public Activity FromActivity { get; set; }
        public Activity ToActivity { get; set; }

        public async Task OnGetAsync(int id, int activityId)
        {
            WorkItem = await FindWorkItemResultAsync(id);

            bool isForward = await GetIsForwardAsync(WorkItem.ActivityId, activityId);

            HandOff = new HandOff()
            {
                WorkItemId = WorkItem.Id,
                FromUserId = UserId,
                FromActivityId = WorkItem.ActivityId,
                ToActivityId = activityId,
                IsForward = isForward // this doesn't render correctly in the form (it comes out as "value" instead of true/false for some reason)
            };
        }

        public SelectList GetAssignToUsers()
        {           
            int userId = (ToActivity.ResponsibilityId == 1) ? 
                (WorkItem.BusinessUserId ?? 0) : 
                (WorkItem.DeveloperUserId ?? 0);

            return new SelectList(AssignToUsers, "Value", "Text", userId);
        }

        public async Task<IActionResult> OnPostAsync(HandOff handOff)
        {
            // model binding makes a bad inference about the Id
            // from the work item Number, so we must clear it to ensure insert
            handOff.Id = 0;

            handOff.IsForward = await GetIsForwardAsync(handOff.FromActivityId, handOff.ToActivityId);

            await handOff.SaveHtmlAsync(Data);

            await Data.TrySaveAsync(handOff);
            return Redirect(ReturnUrl);
        }

        private async Task<bool> GetIsForwardAsync(int fromActivityId, int toActivityId)
        {
            FromActivity = await Data.FindAsync<Activity>(fromActivityId);
            ToActivity = await Data.FindAsync<Activity>(toActivityId);
            return (ToActivity.Order > (FromActivity?.Order ?? 0));
        }
    }
}
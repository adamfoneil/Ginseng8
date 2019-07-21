using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    [Authorize]
    public class AwayHoursModel : AppPageModel
    {
        public AwayHoursModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<VacationHours> AwayHours { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                AwayHours = await new MyVacationHours() { OrgId = OrgId, UserId = UserId, Date = CurrentUser.LocalTime.Date }.ExecuteAsync(cn);
            }
        }

        public async Task<ActionResult> OnPostSave(VacationHours record)
        {
            // had a problem where incoming variable was called "hours"
            // and model binding failed (see https://stackoverflow.com/a/43733129/2023653)

            if (record.Hours > CurrentOrgUser.DailyWorkHours) throw new Exception("Can't be away more than your daily work hours.");

            record.OrganizationId = OrgId;
            record.UserId = UserId;
            await Data.TrySaveAsync(record);
            return RedirectToPage("/Setup/AwayHours");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            await Data.TryDeleteAsync<VacationHours>(id);
            return RedirectToPage("/Setup/AwayHours");
        }
    }
}
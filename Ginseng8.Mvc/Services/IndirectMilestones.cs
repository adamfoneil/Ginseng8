using Ginseng.Models;
using Ginseng.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Postulate.Base;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Classes
{
    public class IndirectMilestones
    {
        public IEnumerable<Option> GetOptions()
        {
            return new Option[]
            {
                new Option() { Value = -1, Text = "End of Week", GetDate = () => DateTime.Today.NextDayOfWeek(DayOfWeek.Friday) },
                new Option() { Value = -2, Text = "Start of Next Week", GetDate = () => DateTime.Today.NextDayOfWeek(DayOfWeek.Monday, 1) },
                new Option() { Value = -3, Text = "End of Next Week", GetDate = () => DateTime.Today.NextDayOfWeek(DayOfWeek.Saturday, 1) },
                new Option() { Value = -4, Text = "End of Month", GetDate = () => DateTime.Today.EndOfMonth() }
            };
        }

        public IEnumerable<SelectListItem> GetSelectListItems() =>
            GetOptions().Select(o => new SelectListItem() { Value = o.Value.ToString(), Text = o.Text });
        
        public Dictionary<int, Option> Options => GetOptions().ToDictionary(row => row.Value);        

        public class Option
        {
            public int Value { get; set; }
            public string Text { get; set; }
            public Func<DateTime> GetDate { get; set; }            

            public async Task<int> GetMilestoneIdAsync(IDbConnection cn, IUser currentUser, int orgId, int teamId)
            {
                var date = GetDate();
                var ms = 
                    await cn.FindWhereAsync<Milestone>(new { OrganizationId = orgId, TeamId = teamId, Date = date }, method: FindWhereMethod.First) ??
                    await cn.FindWhereAsync<Milestone>(new { OrganizationId = orgId, Date = date }, method: FindWhereMethod.First);                

                if (ms == null)
                {
                    ms = new Milestone(date)
                    {
                        OrganizationId = orgId
                    };
                    await Milestone.EnsureUniqueNameAsync(cn, ms);
                    await cn.SaveAsync(ms, currentUser);
                }

                return ms.Id;
            }
        }
    }
}
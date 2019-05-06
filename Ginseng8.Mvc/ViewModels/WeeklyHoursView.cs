using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.Models;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
    public class WeeklyHoursView
    {
        public IEnumerable<CalendarWeeksResult> Weeks { get; set; }
        public IEnumerable<WorkLogsResult> WorkLogs { get; set; }

        public Dictionary<Week, CalendarWeeksResult> WeekInfo
        {
            get
            {
                return Weeks.ToDictionary(row => new Week() { Year = row.Year, WeekNumber = row.WeekNumber });
            }            
        }
    }
}
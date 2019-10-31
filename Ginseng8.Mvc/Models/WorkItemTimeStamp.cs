using Ginseng.Mvc.Queries;
using System;

namespace Ginseng.Mvc.Models
{
    public class WorkItemTimeStamp
    {
        public OpenWorkItemsResult WorkItem { get; set; }
        public DateTime LocalTime { get; set; }
    }
}

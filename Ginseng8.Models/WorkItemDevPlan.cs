using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    /// <summary>
    /// Indicates a day a developer plans to work on an item and for how long
    /// </summary>
    public class WorkItemDevPlan : BaseTable
    {
        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }

        [Column(TypeName = "date")]
        [PrimaryKey]
        public DateTime Date { get; set; }

        public int Hours { get; set; }
    }
}
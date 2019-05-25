using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
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
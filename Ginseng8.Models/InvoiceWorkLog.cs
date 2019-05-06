using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    public class InvoiceWorkLog : BaseTable
    {
        [References(typeof(Invoice))]
        public int InvoiceId { get; set; }

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        /// <summary>
        /// Work must at minimum be related to a project
        /// </summary>
        [References(typeof(Project))]
        public int? ProjectId { get; set; }

        /// <summary>
        /// work hours are usually in reference to a specific work item.
        /// If there's no work item, it means the work relates to project definition/management by itself
        /// </summary>
        [References(typeof(WorkItem))]
        public int? WorkItemId { get; set; }

        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [DecimalPrecision(4, 2)]
        public decimal Hours { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        /// <summary>
        /// if the hours came from a comment or commit message, that's indicated here
        /// </summary>
        public HoursSourceType? SourceType { get; set; }

        /// <summary>
        /// Commit message or comment id that this record was generated from
        /// </summary>
        public int? SourceId { get; set; }
    }
}
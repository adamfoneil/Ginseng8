using Ginseng.Mvc.Interfaces;
using System;

namespace Ginseng.Mvc.Queries.Models
{
    public class WorkLogsResult : IWorkItemTitle, IWorkItemNumber
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int? WorkItemId { get; set; }
        public int ItemId { get; set; } // COALSESCE(WorkItemId, ProjectId)
        public int UserId { get; set; }
        public string DeveloperName { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public int? SourceType { get; set; }
        public int? SourceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public bool IsProject { get; set; }
        public int? WorkItemNumber { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public decimal InvoiceRate { get; set; }
        public decimal Amount { get; set; }
        public int? InvoiceId { get; set; }

        public int Number => WorkItemNumber ?? 0;
        public string DisplayProjectName => Title;
        public int? ProjectPriority => null;
        public int EstimateHours { get; set; }
        public decimal ColorGradientPosition { get; set; }
        public string TeamName { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        int IWorkItemTitle.ProjectId => ProjectId ?? 0;
        int IWorkItemNumber.Number { get { return WorkItemNumber ?? 0; } set { WorkItemNumber = value; } }

        public bool IsEditable(string userName)
        {
            return false;
        }

        public Week ToWeek()
        {
            return new Week() { Year = Year, WeekNumber = WeekNumber };
        }
    }
}
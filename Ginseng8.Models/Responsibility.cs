﻿using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
    /// <summary>
    /// Defines the distinction between Business (Operations) and Development work item responsibility.
    /// As work items change activity, the responsibility changes, which forms the bases of a CASE statement
    /// in the AllWorkItems query that shows the assigned person
    /// </summary>
    public class Responsibility : AppTable
    {
        [PrimaryKey]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Column expression that's valid within the AllWorkItems query that shows the assigned person.
        /// Not used, but my heart was in right place, and it might come back later.
        /// </summary>
        public string SourceExpression { get; set; }

        /// <summary>
        /// Which WorkItem column receives the user Id?
        /// </summary>
        [Required]
        [DefaultExpression("''")]
        [MaxLength(50)]
        public string WorkItemUserIdColumn { get; set; }

        [DefaultExpression("0")]
        public int Flag { get; set; }

        /// <summary>
        /// Indicates whether projects typically align with Freshdesk companies or not    .
        /// The Ops responsibility typically aligns projects with Freshdesk companies, but Dev doesn't
        /// </summary>
        [DefaultExpression("0")]
        public bool CompanySpecificProjects { get; set; }

        [References(typeof(Team))]
        public int? TeamId { get; set; }

        /// <summary>
        /// Indicates what WorkItem property to set according to the Responsibility.Id in effect (which comes from the activity that was selected).
        /// The Id (dictionary key) values are assumed from the order of the seed data records
        /// </summary>
        public static Dictionary<int, Action<WorkItem, int>> SetWorkItemUserActions
        {
            get
            {
                return new Dictionary<int, Action<WorkItem, int>>()
                {
                    { 1, (wi, userId) => wi.BusinessUserId = userId },
                    { 2, (wi, userId) => wi.DeveloperUserId = userId }
                };
            }
        }

        /// <summary>
        /// Indicates what WorkItem property to clear according to the Responsibility.Id in effect
        /// The Id (dictionary key) values are assumed from the order of the seed data records
        /// </summary>
        public static Dictionary<int, Action<WorkItem>> ClearWorkItemUserActions
        {
            get
            {
                return new Dictionary<int, Action<WorkItem>>()
                {
                    { 1, (wi) => wi.BusinessUserId = null },
                    { 2, (wi) => wi.DeveloperUserId = null }
                };
            }
        }

        public static Dictionary<int, string> WorkItemColumnName
        {
            get
            {
                return new Dictionary<int, string>()
                {
                    { 1, nameof(WorkItem.BusinessUserId) },
                    { 2, nameof(WorkItem.DeveloperUserId) }
                };
            }
        }

        public static DataTable GetSeedData()
        {
            return new Responsibility[]
            {
                new Responsibility() { Name = "Operations", SourceExpression = "OwnerName", Flag = 1 },
                new Responsibility() { Name = "Development", SourceExpression = "DeveloperName", Flag = 2 }
            }.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
        }
    }
}
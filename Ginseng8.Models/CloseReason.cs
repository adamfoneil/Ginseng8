﻿using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using Postulate.Base.Extensions;
using Postulate.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Ginseng.Models
{
    public class CloseReason : AppTable
    {
        [MaxLength(50)]
        [PrimaryKey]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public const string Done = "Done";
        public const string Obsolete = "Obsolete";
        public const string Duplicate = "Duplicate";

        public static DataTable GetSeedData()
        {
            return new CloseReason[]
            {
                new CloseReason() { Name = Done, Description = "Work done and changes deployed to production" },
                new CloseReason() { Name = Duplicate, Description = "Redundant to another work item." },
                new CloseReason() { Name = Obsolete, Description = "Requirements out of date." }
            }.ToDataTable(new SqlServerIntegrator(), excludeIdentity: true);
        }
    }
}
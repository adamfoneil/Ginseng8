﻿using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    public class LabelSubscription : BaseTable, INotifyOptions
    {
        [References(typeof(Organization))]
        [PrimaryKey]
        public int OrganizationId { get; set; }

        [PrimaryKey]
        [DefaultExpression("0")]
        public int ApplicationId { get; set; }

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }        

        [References(typeof(Label))]
        [PrimaryKey]
        public int LabelId { get; set; }

        public string TableName => nameof(LabelSubscription);

        public bool SendEmail { get; set; }

        public bool SendText { get; set; }

        public bool InApp { get; set; }

        public bool AllowNotification()
        {
            return NotifyOptionsImplementation.AllowNotification(this);
        }

        [NotMapped]
        public string LabelName { get; set; }

        [NotMapped]
        public string ApplicationName { get; set; }
    }
}
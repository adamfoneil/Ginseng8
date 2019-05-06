using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    public class Invoice : BaseTable
    {
        [PrimaryKey]
        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        [PrimaryKey]
        public int Number { get; set; }

        [References(typeof(Application))]
        public int ApplicationId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [References(typeof(InvoiceStatus))]
        public int StatusId { get; set; }

        public DateTime? StatusDate { get; set; }
    }
}
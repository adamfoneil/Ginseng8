using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
    public class VacationHours : BaseTable
    {
        [References(typeof(Organization))]
        [PrimaryKey]
        public int OrganizationId { get; set; }

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        [PrimaryKey]
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:M/d/yy}")]
        public DateTime Date { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public int Hours { get; set; }
    }
}
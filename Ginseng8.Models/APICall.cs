using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
    [Schema("log")]
    [Identity(nameof(Id))]
    public class APICall
    {
        public int Id { get; set; }

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        [Required]
        public string BaseUrl { get; set; }

        [MaxLength(10)]
        [Required]
        public string Method { get; set; }

        [MaxLength(255)]
        [Required]
        public string Resource { get; set; }

        public string Content { get; set; }

        public bool IsSuccessful { get; set; }

        public string StatusDescription { get; set; }
    }
}
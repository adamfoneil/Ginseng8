using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
    [Schema("git")]
    [Identity(nameof(Id))]
    public class Commit
    {
        public int Id { get; set; }

        public int RepositoryId { get; set; }

        [Required]
        [MaxLength(255)]
        public string RepositoryName { get; set; }

        [MaxLength(50)]
        [Required]
        [UniqueKey]
        public string CommitId { get; set; }

        [MaxLength(50)]
        [Required]
        public string PushRef { get; set; }

        [MaxLength(500)]
        public string BranchName { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        [MaxLength(500)]
        public string Url { get; set; }

        [Required]
        [MaxLength(500)]
        public string AuthorUserName { get; set; }
    }
}
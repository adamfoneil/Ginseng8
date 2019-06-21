using System;

namespace Ginseng.Mvc.Models
{
    public class BlobInfo
    {
        public Uri Uri { get; set; }
        public string Filename { get; set; }
        public long Length { get; set; }
        public DateTimeOffset? LastModified { get; set; }
    }
}
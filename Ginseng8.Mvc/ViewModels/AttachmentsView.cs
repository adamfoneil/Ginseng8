using Ginseng.Mvc.Models;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class AttachmentsView
	{
		public bool AllowDelete { get; set; }
		public IEnumerable<BlobInfo> Blobs { get; set; }
	}
}
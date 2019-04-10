using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class HtmlEditorView
	{
		public int Id { get; set; }
		public string IdPrefix { get; set; }
		public string Content { get; set; }
		public string PostUrl { get; set; }
		public bool AllowComments { get; set; }
		public string UploadFolderName { get; set; }
		public Dictionary<string, object> PostFields { get; set; }
	}
}
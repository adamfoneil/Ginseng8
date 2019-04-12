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

		/// <summary>
		/// Used to inject work item numbers for upload storage path purposes.
		/// Work items are odd in that they have both an Id and Number. The Number is what the user sees/expects,
		/// but the Id ensures editor compatibility in different parts of app.
		/// </summary>
		public int OverrideId { get; set; }

		/// <summary>
		/// This should be used to determine the id of uploaded content
		/// </summary>
		public int UploadId
		{
			get { return (OverrideId != 0) ? OverrideId : Id; }
		}
	}
}
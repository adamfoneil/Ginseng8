using Ginseng.Models;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class InsertItemView
	{
		public InsertItemView()
		{
		}

		public string Context { get; set; }
		public WorkItem WorkItem { get; set; }

		/// <summary>
		/// Hidden fields to render that key to WorkItem properties
		/// </summary>
		public Dictionary<string, object> DefaultValues { get; set; }

		//public SelectList[]
	}
}
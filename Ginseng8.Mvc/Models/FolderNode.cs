using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.Models
{
	public class FolderNode<T>
	{
		public string Name { get; set; }
		public IEnumerable<FolderNode<T>> Children { get; set; }
		public IEnumerable<T> Items { get; set; }

		public static FolderNode<T> ResolveStructure(IEnumerable<T> items, Func<T, string> getPath, char separator)
		{

		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Models
{
	public class Node<T>
	{
		public string Name { get; set; }
		public IEnumerable<Node<T>> Children { get; set; }
		public IEnumerable<T> Items { get; set; }

		public static Node<T> ResolveStructure(IEnumerable<T> items, Func<T, string> getPath, char separator)
		{
			Node<T> result = new Node<T>() { Name = "Root" };

			IEnumerable<string> parsePath(T item)
			{
				return getPath.Invoke(item).Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToString());
			};

			var navs = items.Select(i => new
			{
				Path = string.Join(separator, parsePath(i)), // make sure path has no spaces or repeating separators
				Folders = parsePath(i).ToArray(), // split the folder elements into array
				Item = i
			});
			
			int maxLevel = navs.Max(i => i.Folders.Length);

			for (int level = 0; level < maxLevel - 1; level++)
			{
				List<Node<T>> children = new List<Node<T>>();
				
				var uniqueFolders = navs.GroupBy(nav => string.Join(separator, nav.Folders.Take(level + 1)));
				foreach (var folderGrp in uniqueFolders)
				{					
					Node<T> child = new Node<T>() { Name = folderGrp.Key.Split(separator).Last() };					

					child.Items = folderGrp
						.Where(nav => nav.Path.Equals(folderGrp.Key))
						.Select(nav => nav.Item);

					children.Add(child);
					result.Children = children;
				}
			}

			return result;
		}
	}	
}
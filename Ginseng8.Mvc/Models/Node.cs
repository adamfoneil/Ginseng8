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
			Node<T> root = new Node<T>() { Name = "Root" };

			IEnumerable<string> parsePath(T item)
			{
				return getPath.Invoke(item).Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToString());
			};

			var navs = items.Select(i => new NodeResolver<T>()
			{
				Path = string.Join(separator, parsePath(i)), // make sure path has no spaces or repeating separators
				Folders = parsePath(i).ToArray(), // split the folder elements into array
				Item = i
			});

			var rootFolders = navs.GroupBy(nav => nav.Folders[0]);

			Stack<string> breadcrumb = new Stack<string>();
			ResolveChildren(root, rootFolders, separator, 0, breadcrumb);
			
			return root;
		}

		private static void ResolveChildren(Node<T> parent, IEnumerable<IGrouping<string, NodeResolver<T>>> folders, char separator, int depth, Stack<string> breadcrumb)
		{
			if (!folders.Any()) return;

			List<Node<T>> children = new List<Node<T>>();
			foreach (var folderGrp in folders)
			{
				Node<T> child = new Node<T>() { Name = folderGrp.Key };

				breadcrumb.Push(folderGrp.Key);

				child.Items = folderGrp
						.Where(nav => nav.Path.Equals(string.Join(separator, breadcrumb.Reverse())))
						.Select(nav => nav.Item).ToArray();

				// are there any folders below this?
				if (folderGrp.Any(nav => nav.Folders.Length > depth + 1))
				{
					depth++;
					var subFolders = folderGrp.GroupBy(nav => nav.Folders[depth]).ToArray();
					ResolveChildren(child, subFolders, separator, depth, breadcrumb);
					depth--;
				}

				breadcrumb.Pop();

				children.Add(child);
			}
			parent.Children = children;
		}
	}	

	internal class NodeResolver<T>
	{
		public string Path { get; set; }
		public string[] Folders { get; set; }
		public T Item { get; set; }
	}
}
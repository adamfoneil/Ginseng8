using Ginseng.Mvc.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Testing
{
	[TestClass]
	public class NodeTest
	{
		[TestMethod]
		public void TestNodeResolution()
		{
			var items = new SampleItem[]
			{
				new SampleItem() { Path = "Home", Name = "This" },
				new SampleItem() { Path = "Home", Name = "That"},
				new SampleItem() { Path = "Pushkin", Name = "Fistula" },
				new SampleItem() { Path = "Pushkin/Level1", Name = "Hepsulor" },
				new SampleItem() { Path = "Home", Name = "Other" },
				new SampleItem() { Path = "Away", Name = "Jenga" },
				new SampleItem() { Path = "Home/Level1", Name = "Hello" },
				new SampleItem() { Path = "Home/Level1", Name = "Goodbye" },
				new SampleItem() { Path = "Home/Level2", Name = "Whatever" },
				new SampleItem() { Path = "Home/Level2", Name = "Threnksadoodle" },
				new SampleItem() { Path = "Home/Level3/Aquatics", Name = "Wakwa" },
				new SampleItem() { Path = "Home/Level3/Aquatics", Name = "Yimyim" },
				new SampleItem() { Path = "Away", Name = "Plopsie" }
			};

			var node = Node<SampleItem>.ResolveStructure(items, (i) => i.Path, '/');
		}
	}

	public class SampleItem
	{
		public string Path { get; set; }
		public string Name { get; set; }
	}
}

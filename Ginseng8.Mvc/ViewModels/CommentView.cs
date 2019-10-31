﻿using Ginseng.Models;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class CommentView
	{
		public int ObjectId { get; set; }
		public ObjectType ObjectType { get; set; }
		public IEnumerable<Comment> Comments { get; set; }
        public string IdPrefix { get; set; }
        public bool CommentBoxOpen { get; set; }
	}
}
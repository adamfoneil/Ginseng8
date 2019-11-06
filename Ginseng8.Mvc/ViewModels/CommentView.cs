using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class CommentView : IUserInfo
	{
        public CommentView()
        {
        }

        public CommentView(IUserInfo userInfo)
        {
            UserId = userInfo.UserId;
            OrgId = userInfo.OrgId;
            LocalTime = userInfo.LocalTime;
        }

		public int ObjectId { get; set; }
		public ObjectType ObjectType { get; set; }
		public IEnumerable<Comment> Comments { get; set; }
        public string IdPrefix { get; set; }
        public bool CommentBoxOpen { get; set; }

        public int UserId { get; set; }
        public int OrgId { get; set; }
        public DateTime LocalTime { get; set; }
        public string WorkItemCreatedBy { get; set; }

        public string GetCommentClass(Comment comment) 
        {
            string result = "authored-by-creator";
            if (!comment.CreatedBy.Equals(this.WorkItemCreatedBy))
            {
                result = "authored-by-other";
            }
            return result;
        }

    }
}
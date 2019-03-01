using System;

namespace InstaLike.Web.Models
{
    public class CommentModel
    {
        public int PostID { get; set; }
        public string AuthorNickName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset CommentDate { get; set; }
    }
}
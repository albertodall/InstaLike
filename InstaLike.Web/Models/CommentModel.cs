using System;

namespace InstaLike.Web.Models
{
    public class CommentModel
    {
        public int PostID { get; set; } = 0;
        public string AuthorNickName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset CommentDate { get; set; }
    }
}
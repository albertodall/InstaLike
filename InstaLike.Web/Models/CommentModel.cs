using System;

namespace InstaLike.Web.Models
{
    public class CommentModel
    {
        public string AuthorNickName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset DateTime { get; set; }
    }
}
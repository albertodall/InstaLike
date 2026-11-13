using System;
using InstaLike.Core.Posts;

namespace InstaLike.Web.Models
{
    public sealed class CommentModel
    {
        public PostId? PostID { get; set; }
        public string AuthorNickName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset CommentDate { get; set; }
    }
}

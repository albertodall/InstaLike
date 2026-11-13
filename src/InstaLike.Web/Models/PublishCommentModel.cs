using InstaLike.Core.Posts;

namespace InstaLike.Web.Models
{
    public sealed class PublishCommentModel
    {
        public required PostId PostID { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }
}

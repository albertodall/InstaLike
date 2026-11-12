namespace InstaLike.Web.Models;

public sealed class PublishCommentModel
{
    public int PostID { get; set; }
    public string CommentText { get; set; } = string.Empty;
}

namespace InstaLike.Web.Models
{
    public class PublishCommentModel
    {
        public int PostID { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }
}

namespace InstaLike.Web.Models
{
    public class PublishCommentModel
    {
        public int PostID { get; set; }
        public string CommentText { get; set; }
        public int AuthorID { get; set; }
    }
}

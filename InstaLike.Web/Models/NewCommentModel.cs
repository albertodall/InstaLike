namespace InstaLike.Web.Models
{
    public class NewCommentModel
    {
        public int PostID { get; set; }
        public string CommentText { get; set; }
        public string AuthorNickName { get; set; }
    }
}

using System;

namespace InstaLike.Web.Models
{
    public class PostModel
    {
        public int PostID { get; set; }
        public string AuthorNickName { get; set; }
        public byte[] AuthorProfilePicture { get; set; }
        public DateTimeOffset PostDate { get; set; }
        public byte[] Picture { get; set; }
        public string Text { get; set; }
        public string[] UserLikes { get; set; }
        public CommentModel[] Comments { get; set; }
    }
}

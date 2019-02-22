namespace InstaLike.Core.Commands
{
    public class LikeOrDislikePostCommand : ICommand
    {
        public int PostID { get; }
        public int UserID { get; }

        public LikeOrDislikePostCommand(int postID, int userID)
        {
            PostID = postID;
            UserID = userID;
        }
    }
}

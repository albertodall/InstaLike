namespace InstaLike.Core.Commands
{
    public class LikeOrDislikePostCommand : ICommand
    {
        public int PostID { get; }
        public string Nickname { get; }

        public LikeOrDislikePostCommand(int postID, string nickName)
        {
            PostID = postID;
            Nickname = nickName;
        }
    }
}

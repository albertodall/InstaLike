namespace InstaLike.Core.Commands
{
    public class PublishCommentCommand : ICommand
    {
        public int PostID { get; }
        public string Text { get; }
        public int UserID { get; }

        public PublishCommentCommand(int postID, string text, int userID)
        {
            PostID = postID;
            Text = text;
            UserID = userID;
        }
    }
}

namespace InstaLike.Core.Commands
{
    public class PublishCommentCommand : ICommand
    {
        public int PostID { get; }
        public string Text { get; }
        public string AuthorNickName { get; }

        public PublishCommentCommand(int postID, string text, string nickName)
        {
            PostID = postID;
            Text = text;
            AuthorNickName = nickName;
        }
    }
}

using MediatR;

namespace InstaLike.Core.Events
{
    public class CommentPublishedEvent : INotification
    {
        public string SenderNickname { get; }
        public int PostID { get; }
        public string SenderProfileUrl { get; }
        public string PostUrl { get; }

        public CommentPublishedEvent(string senderNickname, string senderProfileUrl, int postID, string postUrl)
        {
            SenderNickname = senderNickname;
            SenderProfileUrl = senderProfileUrl;
            PostID = postID;
            PostUrl = postUrl;
        }
    }
}

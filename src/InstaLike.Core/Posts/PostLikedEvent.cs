using MediatR;

namespace InstaLike.Core.Posts;

public class PostLikedEvent : INotification
{
    public string SenderNickname { get; }
    public int PostID { get; }
    public string SenderProfileUrl { get; }
    public string PostUrl { get; }

    public PostLikedEvent(string senderNickname, string senderProfileUrl, int postID, string postUrl)
    {
        SenderNickname = senderNickname;
        PostID = postID;
        SenderProfileUrl = senderProfileUrl;
        PostUrl = postUrl;
    }
}

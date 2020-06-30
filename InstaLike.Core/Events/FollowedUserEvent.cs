using MediatR;

namespace InstaLike.Core.Events
{
    public class FollowedUserEvent : INotification
    {
        public string SenderNickname { get; }
        public string SenderProfileUrl { get; }
        public string FollowedNickname { get; }

        public FollowedUserEvent(string senderNickname, string senderProfileUrl, string followedNickname)
        {
            SenderNickname = senderNickname;
            SenderProfileUrl = senderProfileUrl;
            FollowedNickname = followedNickname;
        }
    }
}

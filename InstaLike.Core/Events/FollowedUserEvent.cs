using MediatR;

namespace InstaLike.Core.Events
{
    public class FollowedUserEvent : INotification
    {
        public string SenderNickname { get; set; }
        public string SenderProfileUrl { get; }
        public string FollowingNickname { get; }

        public FollowedUserEvent(string senderNickname, string senderProfileUrl, string followingNickname)
        {
            SenderNickname = senderNickname;
            SenderProfileUrl = senderProfileUrl;
            FollowingNickname = followingNickname;
        }
    }
}

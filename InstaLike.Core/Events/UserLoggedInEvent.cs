using MediatR;

namespace InstaLike.Core.Events
{
    public sealed class UserLoggedInEvent : INotification
    {
        public int UserID { get; }

        public string Nickname { get; }

        public UserLoggedInEvent(int userId, string nickname)
        {
            UserID = userId;
            Nickname = nickname;
        }
    }
}

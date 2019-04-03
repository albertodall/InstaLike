using MediatR;

namespace InstaLike.Core.Events
{
    public sealed class UserLoggedOutEvent : INotification
    {
        public int UserID { get; private set; }

        public string Nickname { get; private set; }

        public UserLoggedOutEvent(int userId, string nickname)
        {
            UserID = userId;
            Nickname = nickname;
        }
    }
}

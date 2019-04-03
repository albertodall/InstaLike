using MediatR;

namespace InstaLike.Core.Events
{
    public sealed class UserLoggedInEvent : INotification
    {
        public int UserID { get; private set; }

        public string Nickname { get; private set; }

        public UserLoggedInEvent(int userId, string nickname)
        {
            UserID = userId;
            Nickname = nickname;
        }
    }
}

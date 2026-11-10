using MediatR;

namespace InstaLike.Core.Accounts;

public sealed class UserLoggedOutEvent : INotification
{
    public int UserID { get; }

    public string Nickname { get; }

    public UserLoggedOutEvent(int userId, string nickname)
    {
        UserID = userId;
        Nickname = nickname;
    }
}

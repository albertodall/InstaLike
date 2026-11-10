using MediatR;

namespace InstaLike.Core.Notifications;

public sealed class MarkAllUserNotificationsReadCommand : IRequest<int>
{
    public int UserID { get; }

    public MarkAllUserNotificationsReadCommand(int userID)
    {
        UserID = userID;
    }

}

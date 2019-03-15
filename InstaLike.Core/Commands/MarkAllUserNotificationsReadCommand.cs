using MediatR;

namespace InstaLike.Core.Commands
{
    public sealed class MarkAllUserNotificationsReadCommand : IRequest<int>
    {
        public int UserID { get; }

        public MarkAllUserNotificationsReadCommand(int userID)
        {
            UserID = userID;
        }

    }
}
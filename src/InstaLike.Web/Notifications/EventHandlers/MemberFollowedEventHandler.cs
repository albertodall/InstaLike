using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Core.Social;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Notifications.EventHandlers
{
    internal sealed class MemberFollowedEventHandler : INotificationHandler<MemberFollowedEvent>
    {
        private readonly ISession _session;

        public MemberFollowedEventHandler(ISession session)
        {
            _session = session;
        }

        public Task Handle(MemberFollowedEvent notification, CancellationToken cancellationToken) =>
            _session.SaveAsync(Notification.Followed(notification.FollowerID, notification.FollowedID), cancellationToken);
    }
}

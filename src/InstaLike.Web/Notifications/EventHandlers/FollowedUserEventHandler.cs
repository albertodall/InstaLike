using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Core.Social;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Notifications.EventHandlers;

internal sealed class FollowedUserEventHandler : INotificationHandler<FollowedUserEvent>
{
    private readonly ISession _session;

    public FollowedUserEventHandler(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public Task Handle(FollowedUserEvent notification, CancellationToken cancellationToken) =>
        _session.SaveAsync(Notification.Followed(notification.FollowerID, notification.FollowedID), cancellationToken);
}

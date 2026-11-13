using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Notifications.EventHandlers;

internal sealed class PostLikedEventHandler : INotificationHandler<PostLikedEvent>
{
    private readonly ISession _session;

    public PostLikedEventHandler(ISession session)
    {
        _session = session;
    }

    public Task Handle(PostLikedEvent notification, CancellationToken cancellationToken) =>
        _session.SaveAsync(Notification.PostLiked(notification.LikerID, notification.PostAuthorID, notification.PostID.Value), cancellationToken);
}

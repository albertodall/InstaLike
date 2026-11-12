using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using MediatR;
using NHibernate;

namespace InstaLike.Web.Notifications.EventHandlers;

internal sealed class CommentPublishedEventHandler : INotificationHandler<CommentPublishedEvent>
{
    private readonly ISession _session;

    public CommentPublishedEventHandler(ISession session)
    {
        _session = session;
    }

    public Task Handle(CommentPublishedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.CommentAuthorID == notification.PostAuthorID)
        {
            return Task.CompletedTask;
        }

        return _session.SaveAsync(
            Notification.PostCommented(notification.CommentAuthorID, notification.PostAuthorID, notification.PostID),
            cancellationToken);
    }
}

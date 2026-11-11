using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Notifications.EventHandlers;

public class CommentPublishedEventHandler : INotificationHandler<CommentPublishedEvent>
{
    private readonly NHibernate.ISession _session;
    private readonly ILogger _logger;

    public CommentPublishedEventHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<CommentPublishedEvent>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CommentPublishedEvent notification, CancellationToken cancellationToken)
    {
        using (var tx = _session.BeginTransaction())
        {
            User? sender = null;
            try
            {
                var postQuery = _session.QueryOver<Post>()
                    .Fetch(SelectMode.Fetch, p => p.Author)
                    .Where(p => p.ID == notification.PostID)
                    .FutureValue();

                var senderQuery = _session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", notification.SenderNickname))
                    .FutureValue();

                var post = await postQuery.GetValueAsync(cancellationToken);
                sender = await senderQuery.GetValueAsync(cancellationToken);
                var notificationToInsert = Notification.PostCommented(sender.ID, post.Author.ID, post.ID);
                await _session.SaveAsync(notificationToInsert, cancellationToken);

                await tx.CommitAsync(cancellationToken);

                _logger.Information("Sent comment notification for post {PostID}. Notification sender: [{SenderNickname}({UserID})]",
                    notification.PostID,
                    notification.SenderNickname,
                    sender.ID);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                _logger.Error("Failed to send comment notification for post {PostID}. Notification sender: [{SenderNickname}({UserID})]. Error message {ErrorMessage}",
                    notification.PostID,
                    notification.SenderNickname,
                    sender?.ID,
                    ex.Message);

                throw;
            }
        }
    }
}

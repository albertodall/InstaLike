using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Core.Events;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.EventHandlers
{
    public class PostLikedEventHandler : INotificationHandler<PostLikedEvent>
    {
        private const string NotificationMessageTemplate = "<a href=\"{0}\">{1}</a> liked your <a href=\"{2}\">post.</a>";

        private readonly ISession _session;
        private readonly ILogger _logger;

        public PostLikedEventHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<PostLikedEvent>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(PostLikedEvent notification, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                User sender = null;
                try
                {
                    var postQuery = _session.QueryOver<Post>()
                        .Fetch(SelectMode.Fetch, p => p.Author)
                        .Where(p => p.ID == notification.PostID)
                        .FutureValue();

                    var senderQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", notification.SenderNickname))
                        .FutureValue();

                    var post = await postQuery.GetValueAsync();
                    var message = string.Format(NotificationMessageTemplate,
                        notification.SenderProfileUrl,
                        notification.SenderNickname,
                        notification.PostUrl);

                    sender = await senderQuery.GetValueAsync();
                    var notificationToInsert = new Notification(sender, post.Author, message);

                    await _session.SaveAsync(notificationToInsert);
                    await tx.CommitAsync();
                    _logger.Information("Sent notification for a 'Like' put to post {PostID} by [{SenderNickName}({UserID})]", 
                        notification.PostID, 
                        notification.SenderNickname,
                        sender.ID);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    _logger.Error("Failed to send notification for a 'Like' put to post {PostID} by [{SenderNickName}({UserID})]. Error message: {ErrorMessage}", 
                        notification.PostID,
                        notification.SenderNickname,
                        sender?.ID,
                        ex.Message);
                    throw;
                }
            }
        }
    }
}
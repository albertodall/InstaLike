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
    public class FollowedUserEventHandler : INotificationHandler<FollowedUserEvent>
    {
        private const string NotificationMessageTemplate = "<a href=\"{0}\">{1}</a> has started following you.";

        private readonly ISession _session;
        private readonly ILogger _logger;

        public FollowedUserEventHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<FollowedUserEventHandler>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(FollowedUserEvent notification, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                User sender = null;
                User recipient = null;
                try
                {
                    var senderQuery = _session.QueryOver<User>()
                        .Where(Restrictions.Eq("Nickname", notification.SenderNickname))
                        .FutureValue();

                    var recipientQuery = _session.QueryOver<User>()
                       .Where(Restrictions.Eq("Nickname", notification.FollowedNickname))
                       .FutureValue();

                    var message = string.Format(NotificationMessageTemplate,
                        notification.SenderProfileUrl,
                        notification.SenderNickname);

                    sender = await senderQuery.GetValueAsync();
                    recipient = await recipientQuery.GetValueAsync();

                    var notificationToInsert = new Notification(sender, recipient, message);
                    await _session.SaveAsync(notificationToInsert);
                    await tx.CommitAsync();

                    _logger.Information("User {FollowedID} ({FollowedNickname}) has been notified about {FollowerID} ({FollowerNickname}) follow.",
                        recipient.ID,
                        notification.FollowedNickname,
                        sender.ID,
                        notification.SenderNickname);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    _logger.Error("Failed to send follow notification to user {FollowedID} ({FollowedNickname}. Error message: {ErrorMessage})", 
                        recipient.ID, 
                        recipient.Nickname,
                        ex.Message);
                    throw;
                }
            }
        }
    }
}

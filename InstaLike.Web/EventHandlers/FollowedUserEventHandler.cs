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

                    var sender = await senderQuery.GetValueAsync();
                    recipient = await recipientQuery.GetValueAsync();

                    var notificationToInsert = new Notification(sender, recipient, message);
                    await tx.CommitAsync();

                    _logger.Information("User [{FollowedNickname}({FollowedID})] has been notified that user [{FollowerNickname}({FollowerID})] has just started following him/her.",
                        notification.FollowedNickname,
                        recipient.ID,
                        sender.Nickname,
                        sender.ID,
                        notification.SenderNickname);
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync();
                    _logger.Error("Failed to send follow notification to user [{FollowedNickname}({FollowedID})]. Error message: {ErrorMessage})", 
                        recipient?.Nickname,
                        recipient?.ID,
                        ex.Message);

                    throw;
                }
            }
        }
    }
}

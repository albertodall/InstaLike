using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Notifications;
using InstaLike.Core.Social;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Notifications.EventHandlers;

public class FollowedUserEventHandler : INotificationHandler<FollowedUserEvent>
{
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
            User? recipient = null;
            try
            {
                var senderQuery = _session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", notification.SenderNickname))
                    .FutureValue();

                var recipientQuery = _session.QueryOver<User>()
                   .Where(Restrictions.Eq("Nickname", notification.FollowedNickname))
                   .FutureValue();

                var sender = await senderQuery.GetValueAsync(cancellationToken);
                recipient = await recipientQuery.GetValueAsync(cancellationToken);
                var notificationToInsert = Notification.Followed(sender.ID, recipient.ID);

                await _session.SaveAsync(notificationToInsert, cancellationToken);
                await tx.CommitAsync(cancellationToken);

                _logger.Information("User [{FollowedNickname}({FollowedID})] has been notified that user [{FollowerNickname}({FollowerID})] has just started following him/her.",
                    notification.FollowedNickname,
                    recipient.ID,
                    sender.Nickname,
                    sender.ID,
                    notification.SenderNickname);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                _logger.Error("Failed to send follow notification to user [{FollowedNickname}({FollowedID})]. Error message: {ErrorMessage})",
                    recipient?.Nickname,
                    recipient?.ID,
                    ex.Message);

                throw;
            }
        }
    }
}

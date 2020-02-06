using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.CommandHandlers
{
    public class MarkAllUserNotificationsReadCommandHandler : IRequestHandler<MarkAllUserNotificationsReadCommand, int>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public MarkAllUserNotificationsReadCommandHandler(ISession session, ILogger logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger?.ForContext<MarkAllUserNotificationsReadCommand>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Handle(MarkAllUserNotificationsReadCommand request, CancellationToken cancellationToken)
        {
            using (var tx = _session.BeginTransaction())
            {
                int unreadNotificationsCount = 0;
                try
                {
                    var notificationsQuery = QueryOver.Of<Notification>()
                        .Where(n => n.Recipient.ID == request.UserID).AndNot(n => n.HasBeenReadByRecipient);

                    var unreadNotificationsCountQuery = notificationsQuery.GetExecutableQueryOver(_session).ToRowCountQuery().FutureValue<int>();
                    var unreadNotificationsQuery = notificationsQuery.GetExecutableQueryOver(_session).Future<Notification>();

                    foreach (var n in (await unreadNotificationsQuery.GetEnumerableAsync(cancellationToken)))
                    {
                        n.MarkAsRead();
                    }

                    await tx.CommitAsync(cancellationToken);
                    unreadNotificationsCount = await unreadNotificationsCountQuery.GetValueAsync(cancellationToken);

                    if (unreadNotificationsCount > 0)
                    {
                        _logger.Information("User {UserID} marked {ReadNotifications} notification as read.",
                            request.UserID,
                            unreadNotificationsCount);
                    }

                    return unreadNotificationsCount;
                }
                catch (ADOException ex)
                {
                    await tx.RollbackAsync(cancellationToken);

                    _logger.Error("Failed to mark {UnreadNotifications} notifications as read for user {UserID}. Error message: {ErrorMessage}",
                        unreadNotificationsCount,
                        request.UserID,
                        ex.Message);

                    throw;
                }
            }
        }
    }
}

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Web.Notifications.Commands;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Notifications.CommandHandlers
{
    internal sealed class MarkNotificationsReadCommandHandler : IRequestHandler<MarkNotificationsReadCommand, int>
    {
        private readonly ISession _session;
        private readonly ILogger _logger;

        public MarkNotificationsReadCommandHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger.ForContext<MarkNotificationsReadCommand>();
        }

        public async Task<int> Handle(MarkNotificationsReadCommand request, CancellationToken cancellationToken)
        {
            if (request.NotificationIDs.Count == 0)
            {
                return 0;
            }

            var unreadNotifications = await _session.QueryOver<Notification>()
                .Where(n => n.RecipientID == request.RecipientID)
                .AndNot(n => n.HasBeenReadByRecipient)
                .AndRestrictionOn(n => n.ID).IsIn(request.NotificationIDs.Select(NotificationId.From).ToArray())
                .ListAsync(cancellationToken);

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
            }

            await _session.FlushAsync(cancellationToken);

            if (unreadNotifications.Count > 0)
            {
                _logger.Information("Member {MemberID} marked {ReadNotifications} notifications as read.",
                    request.RecipientID,
                    unreadNotifications.Count);
            }

            return unreadNotifications.Count;
        }
    }
}

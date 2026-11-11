using System;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Notifications;
using InstaLike.Web.Notifications.Commands;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Notifications.CommandHandlers;

internal sealed class MarkAllUserNotificationsReadCommandHandler : IRequestHandler<MarkAllUserNotificationsReadCommand, int>
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
        var unreadNotifications = await _session.QueryOver<Notification>()
            .Where(n => n.RecipientID == request.UserID)
            .AndNot(n => n.HasBeenReadByRecipient)
            .ListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _session.FlushAsync(cancellationToken);

        if (unreadNotifications.Count > 0)
        {
            _logger.Information("User {UserID} marked {ReadNotifications} notifications as read.",
                request.UserID,
                unreadNotifications.Count);
        }

        return unreadNotifications.Count;
    }
}

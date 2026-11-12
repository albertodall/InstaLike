using System;
using System.Threading.Tasks;
using InstaLike.Web.Extensions;
using InstaLike.Web.Notifications.Commands;
using InstaLike.Web.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers;

[Authorize]
public sealed class NotificationController : Controller
{
    private readonly ISender _dispatcher;

    public NotificationController(ISender dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<IActionResult> Index()
    {
        // Loads all unread notifications
        var query = new NotificationsQuery(User.GetIdentifier(), false);
        var notifications = await _dispatcher.Send(query);

        // Marks all unread notifications as read
        var markAllAsReadCommand = new MarkAllUserNotificationsReadCommand(User.GetIdentifier());
        await _dispatcher.Send(markAllAsReadCommand);

        return View(notifications);
    }
}

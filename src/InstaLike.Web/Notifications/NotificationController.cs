using System;
using System.Threading.Tasks;
using InstaLike.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Notifications
{
    public sealed class NotificationController : Controller
    {
        private readonly ISender _dispatcher;

        public NotificationController(ISender dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<IActionResult> Index()
        {
            var query = new NotificationsQuery(User.GetIdentifier(), false);
            var notifications = await _dispatcher.Send(query);

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(Guid[] notificationIDs)
        {
            var command = new MarkNotificationsReadCommand(User.GetIdentifier(), notificationIDs);
            await _dispatcher.Send(command);

            return RedirectToAction(nameof(Index));
        }
    }
}

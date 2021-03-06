﻿using System;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IMediator _dispatcher;

        public NotificationController(IMediator dispatcher)
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
}
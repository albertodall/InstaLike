using System;
using System.Threading.Tasks;
using InstaLike.Web.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.ViewComponents;

[Authorize]
public sealed class UnreadNotificationsCountViewComponent : ViewComponent
{
    private readonly ISender _dispatcher;

    public UnreadNotificationsCountViewComponent(ISender dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<IViewComponentResult> InvokeAsync(int userID)
    {
        var query = new UnreadNotificationsQuery(userID);
        var count = await _dispatcher.Send(query);

        return View(count);
    }
}

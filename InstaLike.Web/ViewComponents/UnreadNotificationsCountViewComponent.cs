using System;
using System.Threading.Tasks;
using InstaLike.Web.Data.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.ViewComponents
{
    [Authorize]
    public class UnreadNotificationsCountViewComponent : ViewComponent
    {
        private readonly IMediator _dispatcher;

        public UnreadNotificationsCountViewComponent(IMediator dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task<IViewComponentResult> InvokeAsync(int userID)
        {
            var query = new UnreadNotificationsQuery(userID);
            int count = await _dispatcher.Send(query);

            return View(count);
        }
    }
}

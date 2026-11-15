using System.Threading.Tasks;
using InstaLike.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Notifications
{
    public sealed class UnreadNotificationsCountViewComponent : ViewComponent
    {
        private readonly ISender _dispatcher;

        public UnreadNotificationsCountViewComponent(ISender dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var query = new UnreadNotificationsQuery(UserClaimsPrincipal.GetIdentifier());
            var count = await _dispatcher.Send(query);

            return View(count);
        }
    }
}

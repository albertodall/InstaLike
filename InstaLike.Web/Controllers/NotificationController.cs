using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IMediator _dispatcher;

        public NotificationController(IMediator dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Count()
        {
            return new EmptyResult();
        }
    }
}
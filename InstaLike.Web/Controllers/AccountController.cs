using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Models;
using InstaLike.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserAuthenticationService _authenticationService;
        private readonly IMessageDispatcher _dispatcher;

        public AccountController(IUserAuthenticationService authenticationService, IMessageDispatcher dispatcher)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public IActionResult Login(string returnUrl)
        {
            var model = new LoginModel()
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var authenticationResult = await _authenticationService.AuthenticateUser(model.Username, model.Password);
                if (authenticationResult.IsFailure)
                {
                    ModelState.AddModelError("", authenticationResult.Error);
                }

                var userIdentity = new ClaimsIdentity(
                    authenticationResult.Value.Claims, 
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(userIdentity),
                    new AuthenticationProperties()
                    {
                        IsPersistent = model.RememberMe
                    });

                return Redirect(model.ReturnUrl);
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile(string id)
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new RegisterUserCommand(
                    model.Nickname,
                    model.Name,
                    model.Surname,
                    model.Password,
                    model.Email,
                    model.Biography,
                    model.ProfilePicture);

                var processCommandResult = await _dispatcher.DispatchAsync(command);
                if (processCommandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
                
            }
            return View(model);
        }
    }
}
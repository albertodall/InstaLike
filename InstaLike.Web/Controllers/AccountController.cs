using System;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Core.Events;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Extensions;
using InstaLike.Web.Models;
using InstaLike.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private const int MaxThumbnailsInUserProfile = 20;

        private readonly IUserAuthenticationService _authenticationService;
        private readonly IMediator _dispatcher;

        public AccountController(IUserAuthenticationService authenticationService, IMediator dispatcher)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            var model = new LoginModel()
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var authenticationResult = await _authenticationService.AuthenticateUser(model.Username, model.Password);
                if (authenticationResult.IsFailure)
                {
                    ModelState.AddModelError("", authenticationResult.Error);
                }
                else
                {
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

                    await _dispatcher.Publish(new UserLoggedInEvent(authenticationResult.Value.ID, authenticationResult.Value.Nickname));

                    return Redirect(model.ReturnUrl);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userIDClaim = (User.Identity as ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier);
            await _dispatcher.Publish(new UserLoggedOutEvent(int.Parse(userIDClaim.Value), User.Identity.Name));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile(string id)
        {
            var currentUserID = User.GetIdentifier();

            var query = new UserProfileQuery(currentUserID, id, MaxThumbnailsInUserProfile);
            var model = await _dispatcher.Send(query);
            return View(model);
        }

        public async Task<IActionResult> Edit()
        {
            var currentUserID = User.GetIdentifier();

            var query = new UserDetailsQuery(currentUserID);
            var model = await _dispatcher.Send(query);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDetailsModel userDetails, IFormFile profilePictureFile)
        {
            if (ModelState.IsValid)
            {
                var command = new EditUserDetailsCommand(
                    User.GetIdentifier(),
                    userDetails.Nickname,
                    userDetails.Name,
                    userDetails.Surname,
                    userDetails.Email,
                    userDetails.Bio,
                    await profilePictureFile.ToByteArrayAsync());

                var processCommandResult = await _dispatcher.Send(command);
                if (processCommandResult.IsSuccess)
                {
                    return RedirectToAction(nameof(Edit));
                }
                else
                {
                    ModelState.AddModelError("", processCommandResult.Error);
                }
            }
            return View(userDetails);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterUserModel()
            {
                ProfilePicture = Picture.DefaultProfilePicture
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterUserModel newUser, IFormFile profilePictureFile)
        {
            if (ModelState.IsValid)
            {
                var command = new RegisterUserCommand(
                    newUser.Nickname,
                    newUser.Name,
                    newUser.Surname,
                    newUser.Password,
                    newUser.Email,
                    newUser.Biography,
                    await profilePictureFile.ToByteArrayAsync());

                var processCommandResult = await _dispatcher.Send(command);
                if (processCommandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", processCommandResult.Error);
                }
                
            }
            return View(newUser);
        }

        public async Task<IActionResult> Followers(string id)
        {
            ViewBag.Message = $"Users following {id}";
            var query = new FollowersQuery(id);
            var followersList = await _dispatcher.Send(query);
            return PartialView("_UserListPartial", followersList);
        }

        public async Task<IActionResult> Following(string id)
        {
            ViewBag.Message = $"Users followed by {id}";
            var query = new FollowingQuery(id);
            var followersList = await _dispatcher.Send(query);
            return PartialView("_UserListPartial", followersList);
        }

        public async Task<IActionResult> Follow(string id)
        {
            var command = new FollowCommand(User.GetIdentifier(), id);
            var processCommandResult = await _dispatcher.Send(command);

            if (processCommandResult.IsSuccess)
            {
                var newFollowerNotification = new FollowedUserEvent(
                    User.Identity.Name,
                    Url.Action("Profile", "Account", new { id = User.Identity.Name }),
                    id);

                await _dispatcher.Publish(newFollowerNotification);

                return RedirectToAction(nameof(Profile), new { id });
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Unfollow(string id)
        {
            var command = new UnfollowCommand(User.GetIdentifier(), id);
            var processCommandResult = await _dispatcher.Send(command);
            if (processCommandResult.IsSuccess)
            {
                return RedirectToAction(nameof(Profile), new { id });
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
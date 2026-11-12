using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Core.Social;
using InstaLike.Web.Accounts.Commands;
using InstaLike.Web.Accounts.Queries;
using InstaLike.Web.Extensions;
using InstaLike.Web.Models;
using InstaLike.Web.Social.Commands;
using InstaLike.Web.Social.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private const int MaxThumbnailsInUserProfile = 20;

    private readonly ISender _dispatcher;

    public AccountController(ISender dispatcher)
    {
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
            var authenticationResult = await _dispatcher.Send(new LogInCommand(model.Username, model.Password));
            if (authenticationResult.IsFailure)
            {
                ModelState.AddModelError("", authenticationResult.Error);
            }
            else
            {
                var userIdentity = new ClaimsIdentity(
                    GetUserClaims(authenticationResult.Value),
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(userIdentity),
                    new AuthenticationProperties()
                    {
                        IsPersistent = model.RememberMe
                    });

                return Url.IsLocalUrl(model.ReturnUrl)
                    ? LocalRedirect(model.ReturnUrl)
                    : RedirectToAction("Index", "Home");
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _dispatcher.Send(new LogOutCommand(User.GetIdentifier()));

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
    public async Task<IActionResult> Edit(UserDetailsModel userDetails, IFormFile? profilePictureFile)
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
                profilePictureFile is null ? null : await profilePictureFile.ToByteArrayAsync());

            var processCommandResult = await _dispatcher.Send(command);
            if (processCommandResult.IsSuccess)
            {
                return RedirectToAction(nameof(Edit));
            }
            else
            {
                ModelState.AddModelErrors(processCommandResult.Error);
            }
        }

        return View(userDetails);
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterUserModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserModel newUser, IFormFile? profilePictureFile)
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
                profilePictureFile is null ? null : await profilePictureFile.ToByteArrayAsync());

            var processCommandResult = await _dispatcher.Send(command);
            if (processCommandResult.IsSuccess)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelErrors(processCommandResult.Error);
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
        var followedList = await _dispatcher.Send(query);

        return PartialView("_UserListPartial", followedList);
    }

    public async Task<IActionResult> Follow(string id)
    {
        var command = new FollowCommand(User.GetIdentifier(), id);
        var processCommandResult = await _dispatcher.Send(command);

        if (processCommandResult.IsSuccess)
        {
            return RedirectToAction(nameof(Profile), new { id });
        }

        return RedirectToAction("Error", "Home");
    }

    public async Task<IActionResult> Unfollow(string id)
    {
        var command = new UnfollowCommand(User.GetIdentifier(), id);
        var processCommandResult = await _dispatcher.Send(command);

        if (processCommandResult.IsSuccess)
        {
            return RedirectToAction(nameof(Profile), new { id });
        }

        return RedirectToAction("Error", "Home");
    }

    /// <summary>
    /// Builds the list of claims for the authenticated user.
    /// </summary>
    /// <param name="user">Authenticated user.</param>
    /// <returns>The list of claims.</returns>
    private static IEnumerable<Claim> GetUserClaims(User user)
    {
        return new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString(), ClaimValueTypes.Integer32),
            new Claim(ClaimTypes.Name, user.Nickname, ClaimValueTypes.String),
            new Claim(ClaimTypes.GivenName, user.FullName.Name, ClaimValueTypes.String),
            new Claim(ClaimTypes.Surname, user.FullName.Surname, ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email)
        };
    }
}

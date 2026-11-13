using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using InstaLike.Web.Extensions;
using InstaLike.Web.Membership.Queries;
using InstaLike.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Controllers;

[Authorize]
public sealed class AccountController : Controller
{
    private readonly ISender _dispatcher;

    public AccountController(ISender dispatcher)
    {
        _dispatcher = dispatcher;
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
                var memberID = authenticationResult.Value;
                var nickname = await _dispatcher.Send(new MemberNicknameQuery(memberID));
                var memberIdentity = new ClaimsIdentity(
                    GetMemberClaims(memberID, nickname),
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(memberIdentity),
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
        await _dispatcher.Send(new LogOutCommand(AccountId.From(User.GetIdentifier())));

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Builds the list of claims for the authenticated member.
    /// </summary>
    /// <param name="memberID">Identifier of the authenticated member.</param>
    /// <param name="nickname">Nickname of the authenticated member.</param>
    /// <returns>The list of claims.</returns>
    private static IEnumerable<Claim> GetMemberClaims(Guid memberID, string nickname)
    {
        return new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, memberID.ToString(), ClaimValueTypes.String),
            new Claim(ClaimTypes.Name, nickname, ClaimValueTypes.String)
        };
    }
}

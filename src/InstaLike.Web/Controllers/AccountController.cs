using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using InstaLike.Web.Accounts.Queries;
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
        await _dispatcher.Send(new LogOutCommand(CurrentAccountID));

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Settings()
    {
        var email = await _dispatcher.Send(new AccountEmailQuery(CurrentAccountID));
        var model = new AccountSettingsModel()
        {
            ChangeEmail = new ChangeEmailModel() { Email = email }
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeEmail([Bind(Prefix = nameof(AccountSettingsModel.ChangeEmail))] ChangeEmailModel changeEmail)
    {
        if (ModelState.IsValid)
        {
            var result = await _dispatcher.Send(new ChangeEmailCommand(CurrentAccountID, changeEmail.Email));
            if (result.IsSuccess)
            {
                TempData["SettingsMessage"] = "Your e-mail address has been changed.";
                return RedirectToAction(nameof(Settings));
            }

            ModelState.AddModelErrors(result.Error);
        }

        return View(nameof(Settings), new AccountSettingsModel() { ChangeEmail = changeEmail });
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = nameof(AccountSettingsModel.ChangePassword))] ChangePasswordModel changePassword)
    {
        var email = await _dispatcher.Send(new AccountEmailQuery(CurrentAccountID));
        if (ModelState.IsValid)
        {
            var result = await _dispatcher.Send(new ChangePasswordCommand(CurrentAccountID, changePassword.CurrentPassword, changePassword.NewPassword));
            if (result.IsSuccess)
            {
                TempData["SettingsMessage"] = "Your password has been changed.";
                return RedirectToAction(nameof(Settings));
            }

            ModelState.AddModelErrors(result.Error);
        }

        return View(nameof(Settings), new AccountSettingsModel()
        {
            ChangeEmail = new ChangeEmailModel() { Email = email },
            ChangePassword = changePassword
        });
    }

    private AccountId CurrentAccountID => AccountId.From(User.GetIdentifier());

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

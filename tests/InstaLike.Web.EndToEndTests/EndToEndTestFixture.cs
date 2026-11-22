using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using Xunit;

namespace InstaLike.Web.EndToEndTests
{
    [Collection(InstaLikeApplicationFactory.CollectionName)]
    public abstract class EndToEndTestFixture : IAsyncLifetime
    {
        protected const string DefaultPassword = "password";
        protected const string PostDetailUrl = "/Post/Detail/";
        protected const string PostEditUrl = "/Post/Edit/";

        protected EndToEndTestFixture(InstaLikeApplicationFactory application)
        {
            Application = application;
        }

        protected InstaLikeApplicationFactory Application { get; }

        public async ValueTask InitializeAsync()
        {
            await Application.ResetDatabaseAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        /// <summary>
        /// Registers a member with the nickname, logs them in and returns their browser.
        /// </summary>
        protected async Task<Browser> RegisterAndLogInAsync(string nickname)
        {
            var browser = Application.OpenBrowser();
            (await RegisterAsync(browser, nickname)).StatusCode
                .Should()
                .Be(HttpStatusCode.Redirect);
            (await LogInAsync(browser, nickname, DefaultPassword)).StatusCode
                .Should()
                .Be(HttpStatusCode.Redirect);

            return browser;
        }

        /// <summary>
        /// Registers a member with the nickname, from a browser that is closed right away.
        /// </summary>
        protected async Task RegisterMemberAsync(string nickname)
        {
            using var browser = Application.OpenBrowser();
            (await RegisterAsync(browser, nickname)).StatusCode
                .Should()
                .Be(HttpStatusCode.Redirect);
        }

        /// <summary>
        /// Submits the registration form, with the default password and an email address made from the nickname.
        /// </summary>
        protected static async Task<HttpResponseMessage> RegisterAsync(Browser browser, string nickname)
        {
            var registration = await browser.OpenAsync("/Member/Register");

            return await browser.SubmitAsync(Form(registration, "form[action='/Member/Register']"), new Dictionary<string, string>
            {
                ["Nickname"] = nickname,
                ["Name"] = nickname,
                ["Surname"] = "Tester",
                ["Password"] = DefaultPassword,
                ["ConfirmPassword"] = DefaultPassword,
                ["Email"] = $"{nickname}@acme.com",
                ["Biography"] = $"I am {nickname}."
            });
        }

        /// <summary>
        /// Submits the login form with the given credentials.
        /// </summary>
        protected static async Task<HttpResponseMessage> LogInAsync(Browser browser, string username, string password)
        {
            var login = await browser.OpenAsync("/Account/Login");

            return await browser.SubmitAsync(
                Form(login, "form[action^='/Account/Login']"),
                new Dictionary<string, string>
                {
                    ["Username"] = username,
                    ["Password"] = password
                });
        }

        /// <summary>
        /// Publishes a post with the test picture and returns its ID, read from the timeline.
        /// </summary>
        protected static async Task<string> PublishPostAsync(Browser browser, string text)
        {
            var publish = await browser.OpenAsync("/Post/Publish");
            var response = await browser.SubmitWithPictureAsync(
                Form(publish, "form[action='/Post/Publish']"),
                new Dictionary<string, string>
                {
                    ["Text"] = text
                },
                "PictureFile");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);

            var timeline = await browser.FollowRedirectAsync(response);
            var newestPost = timeline.QuerySelector<IHtmlAnchorElement>($".posts a[href^='{PostDetailUrl}']")
                ?? throw new InvalidOperationException("The published post is not on the timeline.");

            return newestPost.PathName[PostDetailUrl.Length..];
        }

        /// <summary>
        /// The page's form matching the CSS selector; fails when there is none.
        /// </summary>
        protected static IHtmlFormElement Form(IHtmlDocument page, string selector)
        {
            return page.QuerySelector<IHtmlFormElement>(selector)
                ?? throw new InvalidOperationException($"The page has no form matching '{selector}'.");
        }
    }
}

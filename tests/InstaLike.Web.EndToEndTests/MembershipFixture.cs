using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using Xunit;

namespace InstaLike.Web.EndToEndTests
{
    public sealed class MembershipFixture : EndToEndTestFixture
    {
        public MembershipFixture(InstaLikeApplicationFactory application)
            : base(application)
        { }

        [Theory]
        [InlineData("/")]
        [InlineData("/Account/Settings")]
        [InlineData("/Member/Edit")]
        [InlineData("/Notification")]
        [InlineData("/Post/Publish")]
        [InlineData("/pictures/posts/00000000-0000-0000-0000-000000000001")]
        public async Task Anonymous_Visitor_Should_Be_Sent_To_The_Login_Page(string url)
        {
            using var browser = Application.OpenBrowser();

            var response = await browser.GetAsync(url);

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location?.OriginalString.Should().StartWith("https://localhost/Account/Login");
            }
        }

        [Theory]
        [InlineData("/Account/Login")]
        [InlineData("/Member/Register")]
        [InlineData("/Home/Error")]
        public async Task Anonymous_Visitor_Should_Open_The_Public_Pages(string url)
        {
            using var browser = Application.OpenBrowser();

            var response = await browser.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Registered_Member_Should_Log_In_And_See_The_Timeline()
        {
            using var browser = await RegisterAndLogInAsync("newmember");

            var timeline = await browser.OpenAsync("/");

            (await OpenOwnProfileAsync(browser, timeline)).Should().Be("/Member/Profile/newmember");
        }

        [Fact]
        public async Task Login_Cookie_Should_Be_Secure_Even_When_Logging_In_Over_HTTP()
        {
            await RegisterMemberAsync("cookiemember");
            using var browser = Application.OpenBrowserOverHttp();

            var response = await LogInAsync(browser, "cookiemember", DefaultPassword);

            var loginCookie = response.Headers.GetValues("Set-Cookie").Single(cookie => cookie.StartsWith(".AspNetCore.Cookies="));
            loginCookie.Split(';', StringSplitOptions.TrimEntries).Should().Contain("secure");
        }

        [Fact]
        public async Task Profile_Link_Should_Follow_A_Nickname_Change()
        {
            using var browser = await RegisterAndLogInAsync("oldnick");
            var edit = await browser.OpenAsync("/Member/Edit");

            var response = await browser.SubmitAsync(
                Form(edit, "form[action='/Member/Edit']"),
                new Dictionary<string, string>
                {
                    ["Nickname"] = "newnick"
                });

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                (await OpenOwnProfileAsync(browser, await browser.OpenAsync("/"))).Should().Be("/Member/Profile/newnick");
            }
        }

        [Fact]
        public async Task Profile_Link_Should_Lead_To_The_Own_Profile_From_Another_Profile()
        {
            await RegisterMemberAsync("someoneelse");

            using var browser = await RegisterAndLogInAsync("myself");
            var otherProfile = await browser.OpenAsync("/Member/Profile/someoneelse");

            (await OpenOwnProfileAsync(browser, otherProfile)).Should().Be("/Member/Profile/myself");
        }

        [Fact]
        public async Task Profile_Of_A_Missing_Member_Should_Not_Be_Found()
        {
            using var browser = await RegisterAndLogInAsync("profilevisitor");

            var response = await browser.GetAsync("/Member/Profile/nobody");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Nickname_Already_Taken_Should_Be_Shown_On_The_Registration_Form()
        {
            await RegisterMemberAsync("nick1");

            using var browser = Application.OpenBrowser();
            var response = await RegisterAsync(browser, "nick1");

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                ValidationErrors(await Browser.ReadPageAsync(response)).Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Wrong_Password_Should_Be_Shown_On_The_Login_Form()
        {
            await RegisterMemberAsync("nick2");

            using var browser = Application.OpenBrowser();
            var response = await LogInAsync(browser, "nick2", "wrong password");

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                ValidationErrors(await Browser.ReadPageAsync(response)).Should().NotBeEmpty();
                (await browser.GetAsync("/")).StatusCode.Should().Be(HttpStatusCode.Redirect);
            }
        }

        [Fact]
        public async Task Repeated_Wrong_Passwords_Should_Lock_The_Account()
        {
            await RegisterMemberAsync("nick_locked");

            using var browser = Application.OpenBrowser();
            for (var attempt = 0; attempt < 5; attempt++)
            {
                (await LogInAsync(browser, "nick_locked", "wrong password")).StatusCode.Should().Be(HttpStatusCode.OK);
            }

            var response = await LogInAsync(browser, "nick_locked", DefaultPassword);

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                ValidationErrors(await Browser.ReadPageAsync(response)).Should().Contain(AccountErrors.LockedOut.Description);
            }
        }

        [Fact]
        public async Task Logged_Out_Member_Should_Be_Sent_To_The_Login_Page()
        {
            using var browser = await RegisterAndLogInAsync("nick3");
            var timeline = await browser.OpenAsync("/");

            var logout = await browser.SubmitAsync(Form(timeline, "form[action='/Account/Logout']"));

            using (new AssertionScope())
            {
                logout.StatusCode.Should().Be(HttpStatusCode.Redirect);
                (await browser.GetAsync("/")).Headers.Location?.OriginalString
                    .Should()
                    .StartWith("https://localhost/Account/Login");
            }
        }

        [Fact]
        public async Task Password_Change_Should_Log_Out_The_Other_Sessions_Only()
        {
            using var browser = await RegisterAndLogInAsync("twodevices");
            using var otherDevice = Application.OpenBrowser();
            (await LogInAsync(otherDevice, "twodevices", DefaultPassword)).StatusCode
                .Should()
                .Be(HttpStatusCode.Redirect);
            var settings = await browser.OpenAsync("/Account/Settings");

            var passwordChange = await browser.SubmitAsync(
                Form(settings, "form[action='/Account/ChangePassword']"),
                new Dictionary<string, string>
                {
                    ["ChangePassword.CurrentPassword"] = DefaultPassword,
                    ["ChangePassword.NewPassword"] = "newpassword",
                    ["ChangePassword.ConfirmNewPassword"] = "newpassword"
                });
            var otherDeviceTimeline = await otherDevice.GetAsync("/");

            using (new AssertionScope())
            {
                passwordChange.StatusCode.Should().Be(HttpStatusCode.Redirect);
                (await browser.GetAsync("/")).StatusCode.Should().Be(HttpStatusCode.OK);
                otherDeviceTimeline.StatusCode.Should().Be(HttpStatusCode.Redirect);
                otherDeviceTimeline.Headers.Location?.OriginalString.Should().StartWith("https://localhost/Account/Login");
            }
        }

        [Fact]
        public async Task Form_Without_A_Valid_Antiforgery_Token_Should_Be_Rejected()
        {
            using var browser = await RegisterAndLogInAsync("nick_forged");
            var timeline = await browser.OpenAsync("/");

            var logout = await browser.SubmitAsync(
                Form(timeline, "form[action='/Account/Logout']"),
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = "nick_forged"
                });

            using (new AssertionScope())
            {
                logout.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                (await browser.GetAsync("/")).StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// Follows the page's link to the member's own profile and returns where it redirects.
        /// </summary>
        private static async Task<string?> OpenOwnProfileAsync(Browser browser, IHtmlDocument page)
        {
            var profileLink = page.QuerySelector<IHtmlAnchorElement>("a[title='User profile']")
                ?? throw new InvalidOperationException("The page has no link to the member's own profile.");
            var response = await browser.GetAsync(profileLink.PathName);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);

            return response.Headers.Location?.OriginalString;
        }

        /// <summary>
        /// The validation messages on the page, under the fields and in the summary.
        /// </summary>
        private static IEnumerable<string> ValidationErrors(IHtmlDocument page)
        {
            return page.QuerySelectorAll(".field-validation-error, .validation-summary-errors li")
                .Select(error => error.TextContent.Trim())
                .Where(error => error.Length > 0);
        }
    }
}

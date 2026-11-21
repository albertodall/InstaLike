using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using FluentAssertions.Execution;
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
                response.Headers.Location?.OriginalString.Should().StartWith("http://localhost/Account/Login");
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

            timeline.QuerySelector<IHtmlAnchorElement>("a[title='User profile']")?.PathName
                .Should()
                .Be("/Member/Profile/newmember");
        }

        [Fact]
        public async Task Nickname_Already_Taken_Should_Be_Shown_On_The_Registration_Form()
        {
            using (await RegisterAndLogInAsync("nick1"))
            { }

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
            using (await RegisterAndLogInAsync("nick2"))
            { }

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
                    .StartWith("http://localhost/Account/Login");
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
                otherDeviceTimeline.Headers.Location?.OriginalString.Should().StartWith("http://localhost/Account/Login");
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

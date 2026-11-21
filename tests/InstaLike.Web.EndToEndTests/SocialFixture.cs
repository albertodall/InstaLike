using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace InstaLike.Web.EndToEndTests
{
    public sealed class SocialFixture : EndToEndTestFixture
    {
        public SocialFixture(InstaLikeApplicationFactory application)
            : base(application)
        { }

        [Fact]
        public async Task Follower_Should_See_The_Posts_Of_The_Followed_Member_Until_Unfollowing()
        {
            using (var author = await RegisterAndLogInAsync("followed"))
            {
                await PublishPostAsync(author, "For my followers");
            }

            using var follower = await RegisterAndLogInAsync("follower");
            var timelineBeforeFollowing = await follower.OpenAsync("/");
            var following = await SubmitProfileButtonAsync(follower, "followed");
            var timelineWhileFollowing = await follower.OpenAsync("/");
            var unfollowing = await SubmitProfileButtonAsync(follower, "followed");
            var timelineAfterUnfollowing = await follower.OpenAsync("/");

            using (new AssertionScope())
            {
                TimelinePosts(timelineBeforeFollowing).Should().BeEmpty();
                following.QuerySelector("form[action='/Member/Unfollow/followed']").Should().NotBeNull();
                TimelinePosts(timelineWhileFollowing).Should().Equal("For my followers");
                unfollowing.QuerySelector("form[action='/Member/Follow/followed']").Should().NotBeNull();
                TimelinePosts(timelineAfterUnfollowing).Should().BeEmpty();
            }
        }

        [Fact]
        public async Task Member_Should_Be_Notified_Of_Followers_Likes_And_Comments_Until_Reading_Them()
        {
            using var author = await RegisterAndLogInAsync("nick10");
            var postID = await PublishPostAsync(author, "Notify me");
            var postDetailUrl = PostDetailUrl + postID;

            using (var fan = await RegisterAndLogInAsync("fan"))
            {
                await SubmitProfileButtonAsync(fan, "nick10");
                var post = await fan.OpenAsync(postDetailUrl);
                await fan.PostFromScriptAsync(post, "/Post/Like", new Dictionary<string, string> { ["PostID"] = postID });
                await fan.PostFromScriptAsync(post, "/Post/PublishComment", new Dictionary<string, string> { ["PostID"] = postID, ["CommentText"] = "Love it" });
            }

            var notifications = await author.OpenAsync("/Notification");
            var markingRead = await author.SubmitAsync(Form(notifications, "form[action='/Notification/MarkRead']"));
            var notificationsAfterReading = await author.FollowRedirectAsync(markingRead);

            using (new AssertionScope())
            {
                UnreadNotificationsCount(notifications).Should().Be("3");
                Notifications(notifications).Should().BeEquivalentTo(
                    "fan has started following you.",
                    "fan liked your post.",
                    "fan wrote a comment about your post.");
                markingRead.StatusCode.Should().Be(HttpStatusCode.Redirect);
                UnreadNotificationsCount(notificationsAfterReading).Should().BeNull();
                notificationsAfterReading.QuerySelector(".alerts .msg")?.TextContent.Should().Be("There are no unread notifications.");
            }
        }

        /// <summary>
        /// Presses the Follow or Unfollow button on the member's profile and returns the profile it leads back to.
        /// </summary>
        private static async Task<IHtmlDocument> SubmitProfileButtonAsync(Browser browser, string nickname)
        {
            var profile = await browser.OpenAsync($"/Member/Profile/{nickname}");
            var response = await browser.SubmitAsync(Form(profile, ".user-profile-info form"));
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);

            return await browser.FollowRedirectAsync(response);
        }

        /// <summary>
        /// The texts of the posts on the timeline.
        /// </summary>
        private static IEnumerable<string> TimelinePosts(IHtmlDocument timeline)
        {
            return timeline.QuerySelectorAll(".posts .post-author-comment").Select(post => post.TextContent);
        }

        /// <summary>
        /// Each notification on the page, as its sender followed by its text.
        /// </summary>
        private static IEnumerable<string> Notifications(IHtmlDocument page)
        {
            return page.QuerySelectorAll(".alerts > div").Select(notification =>
                $"{notification.QuerySelector("a")?.TextContent.Trim()} {notification.QuerySelector("p")?.TextContent.Trim()}");
        }

        /// <summary>
        /// The unread count in the header badge, or null when there is none.
        /// </summary>
        private static string? UnreadNotificationsCount(IHtmlDocument page)
        {
            return page.QuerySelector(".alerts_count")?.TextContent;
        }
    }
}

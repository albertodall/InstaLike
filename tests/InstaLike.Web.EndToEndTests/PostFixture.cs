using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace InstaLike.Web.EndToEndTests
{
    public sealed class PostFixture : EndToEndTestFixture
    {
        public PostFixture(InstaLikeApplicationFactory application)
            : base(application)
        { }

        [Fact]
        public async Task Published_Post_Should_Appear_On_The_Timeline_With_Its_Picture()
        {
            using var browser = await RegisterAndLogInAsync("nick1");

            await PublishPostAsync(browser, "Test picture");

            var timeline = await browser.OpenAsync("/");
            var pictureUrl = timeline.QuerySelector<IHtmlImageElement>(".posts img.image-timeline")?.Source
                ?? throw new InvalidOperationException("The timeline shows no post picture.");
            var picture = await browser.GetAsync(pictureUrl);
            using (new AssertionScope())
            {
                timeline.QuerySelector(".posts .post-author-comment")?.TextContent.Should().Be("Test picture");
                picture.StatusCode.Should().Be(HttpStatusCode.OK);
                picture.Content.Headers.ContentType?.MediaType.Should().Be("image/jpeg");
                (await picture.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken))
                    .Should()
                    .Equal(TestPicture.Content);
            }
        }

        [Fact]
        public async Task Picture_To_Publish_Should_Get_Suggested_Tags()
        {
            using var browser = await RegisterAndLogInAsync("nick2");
            var publish = await browser.OpenAsync("/Post/Publish");

            var response = await browser.UploadPictureFromScriptAsync(publish, "/Post/Autotag");

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                (await response.Content.ReadFromJsonAsync<string[]>(TestContext.Current.CancellationToken)).Should().Equal(FakeImageRecognitionService.ImageTags);
            }
        }

        [Fact]
        public async Task Author_Should_Edit_The_Post_Text()
        {
            using var browser = await RegisterAndLogInAsync("editor");
            var postID = await PublishPostAsync(browser, "Draft");
            var edit = await browser.OpenAsync(PostEditUrl + postID);

            var response = await browser.SubmitWithPictureAsync(
                Form(edit, $"form[action^='{PostEditUrl}']"),
                new Dictionary<string, string>
                {
                    ["Text"] = "Final text"
                },
                "PictureFile");

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                (await browser.OpenAsync(PostDetailUrl + postID)).QuerySelector(".post-author-comment")?.TextContent.Should().Be("Final text");
            }
        }

        [Fact]
        public async Task Other_Members_Should_Not_Edit_The_Post()
        {
            string postID;
            using (var author = await RegisterAndLogInAsync("author"))
            {
                postID = await PublishPostAsync(author, "Mine only");
            }

            using var reader = await RegisterAndLogInAsync("reader");
            var response = await reader.GetAsync(PostEditUrl + postID);
            var accessDenied = await reader.FollowRedirectAsync(response);

            accessDenied.QuerySelector(".msg")?.TextContent
                .Should()
                .Contain("You are not allowed to open this page.");
        }

        [Fact]
        public async Task Other_Members_Should_Not_Edit_The_Post_Through_Their_Own_Edit_Form()
        {
            string postID;
            using (var author = await RegisterAndLogInAsync("victim"))
            {
                postID = await PublishPostAsync(author, "Original text");
            }

            using var tamperer = await RegisterAndLogInAsync("intruder");
            var ownPostID = await PublishPostAsync(tamperer, "My post");
            var ownEdit = await tamperer.OpenAsync(PostEditUrl + ownPostID);

            var response = await tamperer.SubmitWithPictureAsync(
                Form(ownEdit, $"form[action^='{PostEditUrl}']"),
                new Dictionary<string, string>
                {
                    ["PostID"] = postID,
                    ["Text"] = "Tampered text"
                },
                "PictureFile");

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location?.OriginalString.Should().StartWith("https://localhost/Account/AccessDenied");
                (await tamperer.OpenAsync(PostDetailUrl + postID)).QuerySelector(".post-author-comment")?.TextContent.Should().Be("Original text");
            }
        }

        [Fact]
        public async Task Editing_A_Missing_Post_Should_Not_Be_Found()
        {
            using var browser = await RegisterAndLogInAsync("missingpostuser");

            var response = await browser.GetAsync(PostEditUrl + Guid.NewGuid());

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Comment_Should_Be_Shown_Under_The_Post()
        {
            using var browser = await RegisterAndLogInAsync("nick10");
            var postID = await PublishPostAsync(browser, "Here we go!");

            var detailUrl = PostDetailUrl + postID;
            var post = await browser.OpenAsync(detailUrl);

            var response = await browser.PostFromScriptAsync(post, "/Post/PublishComment",
                new Dictionary<string, string>
                {
                    ["PostID"] = postID,
                    ["CommentText"] = "Nice one!"
                });

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                Comments(await browser.OpenAsync(detailUrl)).Should().Equal("Nice one!");
            }
        }

        [Fact]
        public async Task Likes_Should_Be_Counted_On_The_Post()
        {
            string postID;
            using (var author = await RegisterAndLogInAsync("liked"))
            {
                postID = await PublishPostAsync(author, "Like me");
            }

            var postDetailUrl = PostDetailUrl + postID;
            using var fan = await RegisterAndLogInAsync("fan");
            var post = await fan.OpenAsync(postDetailUrl);
            var like = new Dictionary<string, string>
            {
                ["PostID"] = postID
            };

            var liking = await fan.PostFromScriptAsync(post, "/Post/Like", like);
            var likedPost = await fan.OpenAsync(postDetailUrl);
            var disliking = await fan.PostFromScriptAsync(post, "/Post/Dislike", like);
            var dislikedPost = await fan.OpenAsync(postDetailUrl);

            using (new AssertionScope())
            {
                liking.StatusCode.Should().Be(HttpStatusCode.OK);
                LikesCount(likedPost).Should().Be("1");
                likedPost.QuerySelector(".like-button")?.ClassList.Should().Contain("post-liked");
                disliking.StatusCode.Should().Be(HttpStatusCode.OK);
                LikesCount(dislikedPost).Should().Be("0");
            }
        }

        /// <summary>
        /// The texts of the comments on the post page.
        /// </summary>
        private static IEnumerable<string> Comments(IHtmlDocument post)
        {
            return post.QuerySelectorAll(".post-comments > li > p")
                .Select(comment => comment.TextContent.Trim());
        }

        /// <summary>
        /// The likes count on the post page, or null when there is none.
        /// </summary>
        private static string? LikesCount(IHtmlDocument post)
        {
            return post.QuerySelector(".likes-count")?.TextContent;
        }
    }
}

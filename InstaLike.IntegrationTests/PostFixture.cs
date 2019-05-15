using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Web.CommandHandlers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
    public class PostFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public PostFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Publish_A_New_Post()
        {
            string testUserName = "testuser1";
            string testPassword = "password";
            Result<int> publishCommandResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test1 user1", Password.Create(testPassword).Value, (Email)"testuser1@acme.com", "bio1");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }

            var command = new PublishPostCommand(testUser.ID, "test post 1", Convert.FromBase64String(_testFixture.GetTestPictureBase64()));
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new PublishPostCommandHandler(session, Log.Logger);
                publishCommandResult = await sut.Handle(command, default);
            }

            using (new AssertionScope())
            {
                publishCommandResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await session.GetAsync<Post>(publishCommandResult.Value)).Should().NotBeNull();
                }
            }
        }

        [Fact]
        public async Task Should_Put_a_Comment_On_A_Post()
        {
            Result<int> commentPostResult;
            User author = new User((Nickname)"author1", (FullName)"author one", Password.Create("password").Value, (Email)"author1@acme.com", "my bio");
            User commenter = new User((Nickname)"commenter1", (FullName)"commenter one", Password.Create("password").Value, (Email)"commenter1@acme.com", "my bio");
            Post post = new Post(author, (Picture)Convert.FromBase64String(_testFixture.GetTestPictureBase64()), (PostText)"test post");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(author);
                await session.SaveAsync(commenter);
                await session.SaveAsync(post);
            }

            var command = new PublishCommentCommand(post.ID, "test comment", commenter.ID);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new PublishCommentCommandHandler(session, Log.Logger);
                commentPostResult = await sut.Handle(command, default);
            }

            using (new AssertionScope())
            {
                commentPostResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await session.GetAsync<Comment>(commentPostResult.Value)).Should().NotBeNull();
                    (await session.GetAsync<Post>(post.ID)).Comments.Count().Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task Should_Put_A_Like_To_A_Post()
        {
            Result likeResult;
            User author = new User((Nickname)"author2", (FullName)"author two", Password.Create("password").Value, (Email)"author2@acme.com", "my bio");
            User reader = new User((Nickname)"reader1", (FullName)"reader one", Password.Create("password").Value, (Email)"reader1@acme.com", "my bio");
            Post post = new Post(author, (Picture)Convert.FromBase64String(_testFixture.GetTestPictureBase64()), (PostText)"test post");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(author);
                await session.SaveAsync(reader);
                await session.SaveAsync(post);
            }

            var command = new LikePostCommand(post.ID, reader.ID);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new LikeOrDislikePostCommandHandler(session, Log.Logger);
                likeResult = await sut.Handle(command, default);
            }

            using (new AssertionScope())
            {
                likeResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await session.GetAsync<Post>(post.ID)).Likes.Count().Should().Be(1);
                }
            }
        }

        [Fact]
        public async Task Should_Dislike_A_Post()
        {
            Result likeResult;
            User author = new User((Nickname)"author3", (FullName)"author three", Password.Create("password").Value, (Email)"author3@acme.com", "my bio");
            User reader = new User((Nickname)"reader2", (FullName)"reader two", Password.Create("password").Value, (Email)"reader2@acme.com", "my bio");
            Post post = new Post(author, (Picture)Convert.FromBase64String(_testFixture.GetTestPictureBase64()), (PostText)"test post");
            post.PutLikeBy(reader);
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(author);
                await session.SaveAsync(reader);
                await session.SaveAsync(post);
            }

            var command = new DislikePostCommand(post.ID, reader.ID);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new LikeOrDislikePostCommandHandler(session, Log.Logger);
                likeResult = await sut.Handle(command, default);
            }

            using (new AssertionScope())
            {
                likeResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await session.GetAsync<Post>(post.ID)).Likes.Count().Should().Be(0);
                }
            }
        }

        [Fact]
        public async Task Cannot_Dislike_A_Post_Without_Putting_A_Like_First()
        {
            Result likeResult;
            User author = new User((Nickname)"author4", (FullName)"author four", Password.Create("password").Value, (Email)"author4@acme.com", "my bio");
            User reader = new User((Nickname)"reader3", (FullName)"reader three", Password.Create("password").Value, (Email)"reader3@acme.com", "my bio");
            Post post = new Post(author, (Picture)Convert.FromBase64String(_testFixture.GetTestPictureBase64()), (PostText)"test post");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(author);
                await session.SaveAsync(reader);
                await session.SaveAsync(post);
            }

            var command = new DislikePostCommand(post.ID, reader.ID);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new LikeOrDislikePostCommandHandler(session, Log.Logger);
                likeResult = await sut.Handle(command, default);
            }

            likeResult.IsSuccess.Should().BeFalse();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using InstaLike.Web.Data;
using InstaLike.Web.Posts.Commands;
using InstaLike.Web.Social.Commands;
using MediatR;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class DomainEventsFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public DomainEventsFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Liking_A_Post_Should_Notify_Its_Author()
        {
            var (author, reader, post) = await CreatePostAsync("eventauthor1", "eventreader1");

            using (var session = _testFixture.OpenSession(_output))
            {
                (await new TestMediatorBuilder(session).Build().Send(new LikePostCommand(post.ID, reader.ID.Value), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            var notifications = await GetNotificationsAsync(author.ID.Value);
            using (new AssertionScope())
            {
                notifications.Should().ContainSingle();
                notifications[0].Kind.Should().Be(NotificationKind.PostLiked);
                notifications[0].SenderID.Should().Be(reader.ID.Value);
                notifications[0].PostID.Should().Be(post.ID.Value);
            }
        }

        [Fact]
        public async Task Commenting_A_Post_Should_Notify_Its_Author()
        {
            var (author, reader, post) = await CreatePostAsync("eventauthor2", "eventreader2");

            using (var session = _testFixture.OpenSession(_output))
            {
                (await new TestMediatorBuilder(session).Build().Send(new PublishCommentCommand(post.ID, "nice", reader.ID.Value), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            var notifications = await GetNotificationsAsync(author.ID.Value);
            using (new AssertionScope())
            {
                notifications.Should().ContainSingle();
                notifications[0].Kind.Should().Be(NotificationKind.PostCommented);
                notifications[0].SenderID.Should().Be(reader.ID.Value);
                notifications[0].PostID.Should().Be(post.ID.Value);
            }
        }

        [Fact]
        public async Task Commenting_Your_Own_Post_Should_Not_Notify_You()
        {
            var (author, _, post) = await CreatePostAsync("eventauthor3", "eventreader3");

            using (var session = _testFixture.OpenSession(_output))
            {
                (await new TestMediatorBuilder(session).Build().Send(new PublishCommentCommand(post.ID, "my own post", author.ID.Value), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            (await GetNotificationsAsync(author.ID.Value)).Should().BeEmpty();
        }

        [Fact]
        public async Task Following_A_User_Should_Notify_Them()
        {
            var (author, reader, _) = await CreatePostAsync("eventauthor4", "eventreader4");

            using (var session = _testFixture.OpenSession(_output))
            {
                (await new TestMediatorBuilder(session).Build().Send(new FollowCommand(reader.ID.Value, author.Nickname), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            var notifications = await GetNotificationsAsync(author.ID.Value);
            using (new AssertionScope())
            {
                notifications.Should().ContainSingle();
                notifications[0].Kind.Should().Be(NotificationKind.Followed);
                notifications[0].SenderID.Should().Be(reader.ID.Value);
                notifications[0].PostID.Should().BeNull();
            }
        }

        [Fact]
        public async Task A_Failing_Event_Handler_Should_Roll_Back_The_Command()
        {
            var (author, reader, post) = await CreatePostAsync("eventauthor5", "eventreader5");

            using (var session = _testFixture.OpenSession(_output))
            {
                var mediator = new TestMediatorBuilder(session)
                    .WithHandler<INotificationHandler<PostLikedEvent>, FailingHandler>()
                    .Build();

                var like = () => mediator.Send(new LikePostCommand(post.ID, reader.ID.Value));
                await like.Should().ThrowAsync<InvalidOperationException>();
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                using (new AssertionScope())
                {
                    (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
                    (await GetNotificationsAsync(author.ID.Value)).Should().BeEmpty();
                }
            }
        }

        [Fact]
        public async Task A_Rolled_Back_Command_Should_Not_Leak_Its_Events_Into_The_Next_Command()
        {
            var (author, reader, post) = await CreatePostAsync("eventauthor6", "eventreader6");

            using (var session = _testFixture.OpenSession(_output))
            {
                var mediator = new TestMediatorBuilder(session)
                    .WithHandler<IRequestHandler<LikeThenFailCommand, Result>, LikeThenFailCommandHandler>()
                    .Build();

                (await mediator.Send(new LikeThenFailCommand(post.ID, reader.ID.Value), TestContext.Current.CancellationToken)).IsFailure.Should().BeTrue();
                (await mediator.Send(new FollowCommand(reader.ID.Value, author.Nickname), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                using (new AssertionScope())
                {
                    (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
                    (await GetNotificationsAsync(author.ID.Value)).Should().ContainSingle()
                        .Which.Kind.Should().Be(NotificationKind.Followed);
                }
            }
        }

        private async Task<(Member Author, Member Reader, Post Post)> CreatePostAsync(string authorNickname, string readerNickname)
        {
            var author = TestDataHelper.NewMember(authorNickname);
            var reader = TestDataHelper.NewMember(readerNickname);
            Post post;
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(author);
                await session.SaveAsync(reader);
                post = TestDataHelper.NewPost(author, "event post");
                await session.SaveAsync(post);
                await session.FlushAsync();
            }

            return (author, reader, post);
        }

        private async Task<IList<Notification>> GetNotificationsAsync(Guid recipientID)
        {
            using var session = _testFixture.OpenSession(_output);
            return await session.QueryOver<Notification>()
                .Where(n => n.RecipientID == recipientID)
                .ListAsync();
        }

        private sealed class FailingHandler : INotificationHandler<PostLikedEvent>
        {
            public Task Handle(PostLikedEvent notification, CancellationToken cancellationToken) =>
                throw new InvalidOperationException("The notification could not be written.");
        }

        private sealed class LikeThenFailCommand : ICommand<Result>
        {
            public LikeThenFailCommand(PostId postID, Guid userID)
            {
                PostID = postID;
                UserID = userID;
            }

            public PostId PostID { get; }

            public Guid UserID { get; }
        }

        private sealed class LikeThenFailCommandHandler : IRequestHandler<LikeThenFailCommand, Result>
        {
            private readonly NHibernate.ISession _session;

            public LikeThenFailCommandHandler(NHibernate.ISession session)
            {
                _session = session;
            }

            public async Task<Result> Handle(LikeThenFailCommand request, CancellationToken cancellationToken)
            {
                var post = await _session.GetAsync<Post>(request.PostID, cancellationToken);
                post.LikeBy(request.UserID);
                return Result.Failure("The command failed after the post was liked.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Membership;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Data;
using InstaLike.Web.Posts;
using InstaLike.Web.Social;
using MediatR;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class DomainEventsFixture : IntegrationTestFixture
    {
        public DomainEventsFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Liking_A_Post_Should_Notify_Its_Author()
        {
            var (author, reader, post) = await CreatePostAsync("eventauthor1", "eventreader1");

            (await SendAsync(new LikePostCommand(post.ID, reader.ID.Value))).IsSuccess.Should().BeTrue();

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

            (await SendAsync(new PublishCommentCommand(post.ID, "nice", reader.ID.Value))).IsSuccess.Should().BeTrue();

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

            (await SendAsync(new PublishCommentCommand(post.ID, "my own post", author.ID.Value))).IsSuccess.Should().BeTrue();

            (await GetNotificationsAsync(author.ID.Value)).Should().BeEmpty();
        }

        [Fact]
        public async Task Following_A_User_Should_Notify_Them()
        {
            var (author, reader, _) = await CreatePostAsync("eventauthor4", "eventreader4");

            (await SendAsync(new FollowCommand(reader.ID.Value, author.Nickname))).IsSuccess.Should().BeTrue();

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

            using (var session = Infrastructure.OpenSession())
            {
                var mediator = new TestMediatorBuilder(session, Infrastructure.PictureStore)
                    .WithHandler<INotificationHandler<PostLikedEvent>, FailingHandler>()
                    .Build();

                var like = () => mediator.Send(new LikePostCommand(post.ID, reader.ID.Value));
                await like.Should().ThrowAsync<InvalidOperationException>();
            }

            using (var session = Infrastructure.OpenSession())
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

            using (var session = Infrastructure.OpenSession())
            {
                var mediator = new TestMediatorBuilder(session, Infrastructure.PictureStore)
                    .WithHandler<IRequestHandler<LikeThenFailCommand, Result>, LikeThenFailCommandHandler>()
                    .Build();

                (await mediator.Send(new LikeThenFailCommand(post.ID, reader.ID.Value), TestContext.Current.CancellationToken)).IsFailure.Should().BeTrue();
                (await mediator.Send(new FollowCommand(reader.ID.Value, author.Nickname), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            using (var session = Infrastructure.OpenSession())
            {
                using (new AssertionScope())
                {
                    (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
                    (await GetNotificationsAsync(author.ID.Value)).Should().ContainSingle()
                        .Which.Kind.Should().Be(NotificationKind.Followed);
                }
            }
        }

        [Fact]
        public async Task A_Failed_Command_That_Commits_On_Failure_Should_Keep_Its_Changes_And_Events()
        {
            var (author, reader, post) = await CreatePostAsync("eventauthor7", "eventreader7");

            using (var session = Infrastructure.OpenSession())
            {
                var mediator = new TestMediatorBuilder(session, Infrastructure.PictureStore)
                    .WithHandler<IRequestHandler<LikeThenFailCommittedCommand, Result>, LikeThenFailCommittedCommandHandler>()
                    .Build();

                (await mediator.Send(new LikeThenFailCommittedCommand(post.ID, reader.ID.Value), TestContext.Current.CancellationToken)).IsFailure.Should().BeTrue();
            }

            using (var session = Infrastructure.OpenSession())
            {
                using (new AssertionScope())
                {
                    (await session.QueryOver<Like>().Where(l => l.Post.ID == post.ID).RowCountAsync(TestContext.Current.CancellationToken)).Should().Be(1);
                    (await GetNotificationsAsync(author.ID.Value)).Should().ContainSingle()
                        .Which.Kind.Should().Be(NotificationKind.PostLiked);
                }
            }
        }

        /// <summary>
        /// Saves an author, a reader and a post by the author.
        /// </summary>
        private async Task<(Member Author, Member Reader, Post Post)> CreatePostAsync(string authorNickname, string readerNickname)
        {
            var author = TestDataHelper.NewMember(authorNickname);
            var reader = TestDataHelper.NewMember(readerNickname);
            Post post;
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(author);
                await session.SaveAsync(reader);
                post = TestDataHelper.NewPost(author, "event post");
                await session.SaveAsync(post);
                await session.FlushAsync();
            }

            return (author, reader, post);
        }

        /// <summary>
        /// The notifications saved for the recipient.
        /// </summary>
        private async Task<IList<Notification>> GetNotificationsAsync(Guid recipientID)
        {
            using var session = Infrastructure.OpenSession();
            return await session.QueryOver<Notification>()
                .Where(n => n.RecipientID == recipientID)
                .ListAsync();
        }

        /// <summary>
        /// Fails on every liked post, to make the command that raised the event fail.
        /// </summary>
        private sealed class FailingHandler : INotificationHandler<PostLikedEvent>
        {
            public Task Handle(PostLikedEvent notification, CancellationToken cancellationToken) =>
                throw new InvalidOperationException("The notification could not be written.");
        }

        /// <summary>
        /// A command that likes a post and then fails.
        /// </summary>
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

        /// <summary>
        /// Likes the post, raising its event, and then returns a failure,
        /// so the command is rolled back.
        /// </summary>
        private sealed class LikeThenFailCommandHandler : IRequestHandler<LikeThenFailCommand, Result>
        {
            private readonly NHibernate.ISession _session;

            public LikeThenFailCommandHandler(NHibernate.ISession session)
            {
                _session = session;
            }

            public Task<Result> Handle(LikeThenFailCommand request, CancellationToken cancellationToken) =>
                LikeThenFailAsync(_session, request.PostID, request.UserID, cancellationToken);
        }

        /// <summary>
        /// A command that likes a post and then fails, but commits its failures.
        /// </summary>
        [CommitOnFailure]
        private sealed class LikeThenFailCommittedCommand : ICommand<Result>
        {
            public LikeThenFailCommittedCommand(PostId postID, Guid userID)
            {
                PostID = postID;
                UserID = userID;
            }

            public PostId PostID { get; }

            public Guid UserID { get; }
        }

        /// <summary>
        /// Likes the post, raising its event, and then returns a failure, which is committed anyway.
        /// </summary>
        private sealed class LikeThenFailCommittedCommandHandler : IRequestHandler<LikeThenFailCommittedCommand, Result>
        {
            private readonly NHibernate.ISession _session;

            public LikeThenFailCommittedCommandHandler(NHibernate.ISession session)
            {
                _session = session;
            }

            public Task<Result> Handle(LikeThenFailCommittedCommand request, CancellationToken cancellationToken) =>
                LikeThenFailAsync(_session, request.PostID, request.UserID, cancellationToken);
        }

        /// <summary>
        /// Likes the post, raising its event, and then fails.
        /// </summary>
        private static async Task<Result> LikeThenFailAsync(NHibernate.ISession session, PostId postID, Guid userID, CancellationToken cancellationToken)
        {
            var post = await session.GetAsync<Post>(postID, cancellationToken);
            post.LikeBy(userID);
            return Result.Failure(Error.Failure("Test.Failed", "The command failed after the post was liked."));
        }
    }
}

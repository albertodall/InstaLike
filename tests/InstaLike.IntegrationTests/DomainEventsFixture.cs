using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Notifications;
using InstaLike.Core.Posts;
using InstaLike.Core.SharedKernel;
using InstaLike.Core.Social;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests;

public class DomainEventsFixture : IClassFixture<DatabaseFixture>
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
            (await DatabaseFixture.CreateMediator(session).Send(new LikePostCommand(post.ID, reader.ID))).IsSuccess.Should().BeTrue();
        }

        var notifications = await GetNotificationsAsync(author.ID);
        using (new AssertionScope())
        {
            notifications.Should().ContainSingle();
            notifications[0].Kind.Should().Be(NotificationKind.PostLiked);
            notifications[0].SenderID.Should().Be(reader.ID);
            notifications[0].PostID.Should().Be(post.ID);
        }
    }

    [Fact]
    public async Task Commenting_A_Post_Should_Notify_Its_Author()
    {
        var (author, reader, post) = await CreatePostAsync("eventauthor2", "eventreader2");

        using (var session = _testFixture.OpenSession(_output))
        {
            (await DatabaseFixture.CreateMediator(session).Send(new PublishCommentCommand(post.ID, "nice", reader.ID))).IsSuccess.Should().BeTrue();
        }

        var notifications = await GetNotificationsAsync(author.ID);
        using (new AssertionScope())
        {
            notifications.Should().ContainSingle();
            notifications[0].Kind.Should().Be(NotificationKind.PostCommented);
            notifications[0].SenderID.Should().Be(reader.ID);
            notifications[0].PostID.Should().Be(post.ID);
        }
    }

    [Fact]
    public async Task Commenting_Your_Own_Post_Should_Not_Notify_You()
    {
        var (author, _, post) = await CreatePostAsync("eventauthor3", "eventreader3");

        using (var session = _testFixture.OpenSession(_output))
        {
            (await DatabaseFixture.CreateMediator(session).Send(new PublishCommentCommand(post.ID, "my own post", author.ID))).IsSuccess.Should().BeTrue();
        }

        (await GetNotificationsAsync(author.ID)).Should().BeEmpty();
    }

    [Fact]
    public async Task Following_A_User_Should_Notify_Them()
    {
        var (author, reader, _) = await CreatePostAsync("eventauthor4", "eventreader4");

        using (var session = _testFixture.OpenSession(_output))
        {
            (await DatabaseFixture.CreateMediator(session).Send(new FollowCommand(reader.ID, author.Nickname))).IsSuccess.Should().BeTrue();
        }

        var notifications = await GetNotificationsAsync(author.ID);
        using (new AssertionScope())
        {
            notifications.Should().ContainSingle();
            notifications[0].Kind.Should().Be(NotificationKind.Followed);
            notifications[0].SenderID.Should().Be(reader.ID);
            notifications[0].PostID.Should().BeNull();
        }
    }

    [Fact]
    public async Task A_Failing_Event_Handler_Should_Roll_Back_The_Command()
    {
        var (author, reader, post) = await CreatePostAsync("eventauthor5", "eventreader5");

        using (var session = _testFixture.OpenSession(_output))
        {
            var mediator = DatabaseFixture.CreateMediator(session, configure: services =>
                services.AddTransient<INotificationHandler<PostLikedEvent>, FailingHandler>());

            var like = () => mediator.Send(new LikePostCommand(post.ID, reader.ID));
            await like.Should().ThrowAsync<InvalidOperationException>();
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            using (new AssertionScope())
            {
                (await session.QueryOver<Like>().Where(l => l.PostID == post.ID).RowCountAsync()).Should().Be(0);
                (await GetNotificationsAsync(author.ID)).Should().BeEmpty();
            }
        }
    }

    private async Task<(User Author, User Reader, Post Post)> CreatePostAsync(string authorNickname, string readerNickname)
    {
        var author = new User((Nickname)authorNickname, (FullName)"event author", Password.Create("password").Value, (Email)$"{authorNickname}@acme.com", "bio");
        var reader = new User((Nickname)readerNickname, (FullName)"event reader", Password.Create("password").Value, (Email)$"{readerNickname}@acme.com", "bio");
        Post post;
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(author);
            await session.SaveAsync(reader);
            post = new Post(author.ID, PictureId.New(), (PostText)"event post");
            await session.SaveAsync(post);
            await session.FlushAsync();
        }

        return (author, reader, post);
    }

    private async Task<IList<Notification>> GetNotificationsAsync(int recipientID)
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
}

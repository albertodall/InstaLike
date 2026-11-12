using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Notifications;
using InstaLike.Web.Models;
using InstaLike.Web.Notifications.Commands;
using InstaLike.Web.Notifications.Queries;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests;

public sealed class NotificationFixture : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _testFixture;
    private readonly ITestOutputHelper _output;

    public NotificationFixture(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _testFixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Should_Mark_All_Notifications_Read()
    {
        int result;
        var sender = TestDataHelper.NewUser("sender");
        var recipient = TestDataHelper.NewUser("recipient");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(sender, TestContext.Current.CancellationToken);
            await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
            var not1 = Notification.Followed(sender.ID, recipient.ID);
            var not2 = Notification.Followed(sender.ID, recipient.ID);
            var not3 = Notification.Followed(sender.ID, recipient.ID);
            await session.SaveAsync(not1, TestContext.Current.CancellationToken);
            await session.SaveAsync(not2, TestContext.Current.CancellationToken);
            await session.SaveAsync(not3, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new MarkAllUserNotificationsReadCommand(recipient.ID);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).Build();
            result = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        result.Should().Be(3);
    }

    [Fact]
    public async Task Should_Count_Unread_Notifications()
    {
        int result;
        var sender = TestDataHelper.NewUser("sender");
        var recipient = TestDataHelper.NewUser("recipient");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(sender, TestContext.Current.CancellationToken);
            await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
            var not1 = Notification.Followed(sender.ID, recipient.ID);
            var not2 = Notification.Followed(sender.ID, recipient.ID);
            var not3 = Notification.Followed(sender.ID, recipient.ID);
            not1.MarkAsRead();
            await session.SaveAsync(not1, TestContext.Current.CancellationToken);
            await session.SaveAsync(not2, TestContext.Current.CancellationToken);
            await session.SaveAsync(not3, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new UnreadNotificationsQuery(recipient.ID);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new UnreadNotificationsQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        result.Should().Be(2);
    }

    [Fact]
    public async Task Should_Read_All_Notifications()
    {
        NotificationModel[] result;
        var sender = TestDataHelper.NewUser("sender");
        var recipient = TestDataHelper.NewUser("recipient");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(sender, TestContext.Current.CancellationToken);
            await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
            var not1 = Notification.Followed(sender.ID, recipient.ID);
            var not2 = Notification.Followed(sender.ID, recipient.ID);
            var not3 = Notification.Followed(sender.ID, recipient.ID);
            not1.MarkAsRead();
            await session.SaveAsync(not1, TestContext.Current.CancellationToken);
            await session.SaveAsync(not2, TestContext.Current.CancellationToken);
            await session.SaveAsync(not3, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new NotificationsQuery(recipient.ID, true);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new NotificationsQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            result.Length.Should().Be(3);
            result.Should().OnlyContain(n => n.SenderNickname == sender.Nickname && n.Kind == NotificationKind.Followed);
        }
    }

    [Fact]
    public async Task Should_Read_Only_Unread_Notifications()
    {
        NotificationModel[] result;
        var sender = TestDataHelper.NewUser("sender");
        var recipient = TestDataHelper.NewUser("recipient");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(sender, TestContext.Current.CancellationToken);
            await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
            var not1 = Notification.Followed(sender.ID, recipient.ID);
            var not2 = Notification.Followed(sender.ID, recipient.ID);
            var not3 = Notification.Followed(sender.ID, recipient.ID);
            not1.MarkAsRead();
            await session.SaveAsync(not1, TestContext.Current.CancellationToken);
            await session.SaveAsync(not2, TestContext.Current.CancellationToken);
            await session.SaveAsync(not3, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var query = new NotificationsQuery(recipient.ID, false);
        using (var session = _testFixture.OpenStatelessSession())
        {
            var sut = new NotificationsQueryHandler(session, Log.Logger);
            result = await sut.Handle(query, TestContext.Current.CancellationToken);
        }

        result.Length.Should().Be(2);
    }
}

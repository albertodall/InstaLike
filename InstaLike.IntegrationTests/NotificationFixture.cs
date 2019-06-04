using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InstaLike.Core.Domain;
using InstaLike.Web.Data.Query;
using InstaLike.Web.Models;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
    public class NotificationFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public NotificationFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Count_Unread_Notifications()
        {
            int result = 0;
            User sender = new User((Nickname)"sender", (FullName)"sender user", Password.Create("password").Value, (Email)"sender@acme.com", "my bio");
            User recipient = new User((Nickname)"recipient", (FullName)"recipient user", Password.Create("password").Value, (Email)"follower@acme.com", "my bio");
            var not1 = new Notification(sender, recipient, "Notification 1");
            var not2 = new Notification(sender, recipient, "Notification 2");
            var not3 = new Notification(sender, recipient, "Notification 3");

            using (var session = _testFixture.OpenSession(_output))
            {
                not1.MarkAsRead();
                await session.SaveAsync(sender);
                await session.SaveAsync(recipient);
                await session.SaveAsync(not1);
                await session.SaveAsync(not2);
                await session.SaveAsync(not3);
                await session.FlushAsync();
            }

            var query = new UnreadNotificationsQuery(recipient.ID);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new UnreadNotificationsQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, default);
            }

            result.Should().Be(2);
        }

        [Fact]
        public async Task Should_Read_All_Notifications()
        {
            NotificationModel[] result;
            User sender = new User((Nickname)"sender", (FullName)"sender user", Password.Create("password").Value, (Email)"sender@acme.com", "my bio");
            User recipient = new User((Nickname)"recipient", (FullName)"recipient user", Password.Create("password").Value, (Email)"follower@acme.com", "my bio");
            var not1 = new Notification(sender, recipient, "Notification 1");
            var not2 = new Notification(sender, recipient, "Notification 2");
            var not3 = new Notification(sender, recipient, "Notification 3");

            using (var session = _testFixture.OpenSession(_output))
            {
                not1.MarkAsRead();
                await session.SaveAsync(sender);
                await session.SaveAsync(recipient);
                await session.SaveAsync(not1);
                await session.SaveAsync(not2);
                await session.SaveAsync(not3);
                await session.FlushAsync();
            }

            var query = new NotificationsQuery(recipient.ID, true);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new NotificationsQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, default);
            }

            result.Count().Should().Be(3);
        }

        [Fact]
        public async Task Should_Read_Only_Unread_Notifications()
        {
            NotificationModel[] result;
            User sender = new User((Nickname)"sender", (FullName)"sender user", Password.Create("password").Value, (Email)"sender@acme.com", "my bio");
            User recipient = new User((Nickname)"recipient", (FullName)"recipient user", Password.Create("password").Value, (Email)"follower@acme.com", "my bio");
            var not1 = new Notification(sender, recipient, "Notification 1");
            var not2 = new Notification(sender, recipient, "Notification 2");
            var not3 = new Notification(sender, recipient, "Notification 3");

            using (var session = _testFixture.OpenSession(_output))
            {
                not1.MarkAsRead();
                await session.SaveAsync(sender);
                await session.SaveAsync(recipient);
                await session.SaveAsync(not1);
                await session.SaveAsync(not2);
                await session.SaveAsync(not3);
                await session.FlushAsync();
            }

            var query = new NotificationsQuery(recipient.ID, false);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new NotificationsQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, default);
            }

            result.Count().Should().Be(2);
        }
    }
}
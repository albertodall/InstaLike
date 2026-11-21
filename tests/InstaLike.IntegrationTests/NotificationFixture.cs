using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Notifications;
using InstaLike.Web.Notifications;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class NotificationFixture : IntegrationTestFixture
    {
        public NotificationFixture(TestInfrastructureFactory infrastructure)
            : base(infrastructure)
        { }

        [Fact]
        public async Task Should_Mark_The_Given_Notifications_Read()
        {
            int result;
            var sender = TestDataHelper.NewMember("sender");
            var recipient = TestDataHelper.NewMember("recipient");
            var not1 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
            var not2 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
            var not3 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(sender, TestContext.Current.CancellationToken);
                await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
                await session.SaveAsync(not1, TestContext.Current.CancellationToken);
                await session.SaveAsync(not2, TestContext.Current.CancellationToken);
                await session.SaveAsync(not3, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new MarkNotificationsReadCommand(recipient.ID.Value, [not1.ID.Value, not2.ID.Value]);
            result = await SendAsync(command);

            using (new AssertionScope())
            {
                result.Should().Be(2);
                using (var session = Infrastructure.OpenSession())
                {
                    (await session.GetAsync<Notification>(not3.ID, TestContext.Current.CancellationToken))!
                        .HasBeenReadByRecipient.Should().BeFalse();
                }
            }
        }

        [Fact]
        public async Task Should_Not_Mark_Notifications_Of_Another_Member_Read()
        {
            int result;
            var sender = TestDataHelper.NewMember("sender");
            var recipient = TestDataHelper.NewMember("recipient");
            var notification = Notification.Followed(sender.ID.Value, recipient.ID.Value);
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(sender, TestContext.Current.CancellationToken);
                await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
                await session.SaveAsync(notification, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var command = new MarkNotificationsReadCommand(sender.ID.Value, [notification.ID.Value]);
            result = await SendAsync(command);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Should_Count_Unread_Notifications()
        {
            int result;
            var sender = TestDataHelper.NewMember("sender");
            var recipient = TestDataHelper.NewMember("recipient");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(sender, TestContext.Current.CancellationToken);
                await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
                var not1 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                var not2 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                var not3 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                not1.MarkAsRead();
                await session.SaveAsync(not1, TestContext.Current.CancellationToken);
                await session.SaveAsync(not2, TestContext.Current.CancellationToken);
                await session.SaveAsync(not3, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new UnreadNotificationsQuery(recipient.ID.Value);
            using (var session = Infrastructure.OpenStatelessSession())
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
            var sender = TestDataHelper.NewMember("sender");
            var recipient = TestDataHelper.NewMember("recipient");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(sender, TestContext.Current.CancellationToken);
                await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
                var not1 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                var not2 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                var not3 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                not1.MarkAsRead();
                await session.SaveAsync(not1, TestContext.Current.CancellationToken);
                await session.SaveAsync(not2, TestContext.Current.CancellationToken);
                await session.SaveAsync(not3, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new NotificationsQuery(recipient.ID.Value, true);
            using (var session = Infrastructure.OpenStatelessSession())
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
            var sender = TestDataHelper.NewMember("sender");
            var recipient = TestDataHelper.NewMember("recipient");
            using (var session = Infrastructure.OpenSession())
            {
                await session.SaveAsync(sender, TestContext.Current.CancellationToken);
                await session.SaveAsync(recipient, TestContext.Current.CancellationToken);
                var not1 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                var not2 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                var not3 = Notification.Followed(sender.ID.Value, recipient.ID.Value);
                not1.MarkAsRead();
                await session.SaveAsync(not1, TestContext.Current.CancellationToken);
                await session.SaveAsync(not2, TestContext.Current.CancellationToken);
                await session.SaveAsync(not3, TestContext.Current.CancellationToken);
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            var query = new NotificationsQuery(recipient.ID.Value, false);
            using (var session = Infrastructure.OpenStatelessSession())
            {
                var sut = new NotificationsQueryHandler(session, Log.Logger);
                result = await sut.Handle(query, TestContext.Current.CancellationToken);
            }

            result.Length.Should().Be(2);
        }
    }
}

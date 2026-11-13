using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public sealed class AccountAuthenticationFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public AccountAuthenticationFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Registered_Member_Should_Be_Able_To_Log_In_With_The_Username()
        {
            var member = await SaveMemberWithAccountAsync("testuser2", "password");
            Result<Guid> authenticationResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();
                authenticationResult = await sut.Send(new LogInCommand("testuser2", "password"), TestContext.Current.CancellationToken);
            }

            using (new AssertionScope())
            {
                authenticationResult.IsSuccess.Should().BeTrue();
                authenticationResult.Value.Should().Be(member.ID.Value);
            }
        }

        [Fact]
        public async Task Member_Should_Still_Log_In_With_The_Username_After_Changing_The_Nickname()
        {
            var member = await SaveMemberWithAccountAsync("testuser5", "password5");
            using (var session = _testFixture.OpenSession(_output))
            {
                var storedMember = await session.GetAsync<Member>(member.ID, TestContext.Current.CancellationToken);
                storedMember.ChangeNickname((Nickname)"renamed5");
                await session.FlushAsync(TestContext.Current.CancellationToken);
            }

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();

                using (new AssertionScope())
                {
                    (await sut.Send(new LogInCommand("testuser5", "password5"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
                    (await sut.Send(new LogInCommand("renamed5", "password5"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
                }
            }
        }

        [Fact]
        public async Task Registered_Member_Should_Not_Be_Able_To_Log_In_With_Wrong_Password()
        {
            await SaveMemberWithAccountAsync("testuser3", "password3");
            Result<Guid> authenticationResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();
                authenticationResult = await sut.Send(new LogInCommand("testuser3", "xxx42"), TestContext.Current.CancellationToken);
            }

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Unknown_Username_Should_Not_Be_Able_To_Log_In()
        {
            Result<Guid> authenticationResult;

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session).Build();
                authenticationResult = await sut.Send(new LogInCommand("not_existing_user", "xxx42"), TestContext.Current.CancellationToken);
            }

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Logging_In_And_Out_Should_Raise_Events()
        {
            var member = await SaveMemberWithAccountAsync("testuser4", "password4");
            var accountID = AccountId.From(member.ID.Value);

            var raised = new List<IDomainEvent>();
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new TestMediatorBuilder(session)
                    .WithHandler<INotificationHandler<AccountLoggedInEvent>>(new RecordingHandler<AccountLoggedInEvent>(raised))
                    .WithHandler<INotificationHandler<AccountLoggedOutEvent>>(new RecordingHandler<AccountLoggedOutEvent>(raised))
                    .Build();

                (await sut.Send(new LogInCommand("testuser4", "wrong"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
                (await sut.Send(new LogInCommand("testuser4", "password4"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
                (await sut.Send(new LogOutCommand(accountID), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            }

            raised.Should().Equal(new AccountLoggedInEvent(accountID), new AccountLoggedOutEvent(accountID));
        }

        private async Task<Member> SaveMemberWithAccountAsync(string nickname, string password)
        {
            var member = TestDataHelper.NewMember(nickname);
            using var session = _testFixture.OpenSession(_output);
            await session.SaveAsync(member, TestContext.Current.CancellationToken);
            await session.SaveAsync(TestDataHelper.NewAccountWithPassword(member, password), TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
            return member;
        }

        private sealed class RecordingHandler<TEvent> : INotificationHandler<TEvent>
            where TEvent : IDomainEvent
        {
            private readonly List<IDomainEvent> _raised;

            public RecordingHandler(List<IDomainEvent> raised) => _raised = raised;

            public Task Handle(TEvent notification, CancellationToken cancellationToken)
            {
                _raised.Add(notification);
                return Task.CompletedTask;
            }
        }
    }
}

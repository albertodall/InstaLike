using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InstaLike.IntegrationTests;

public class UserAuthenticationFixture : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _testFixture;
    private readonly ITestOutputHelper _output;

    public UserAuthenticationFixture(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _testFixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Registered_User_Should_Be_Able_To_Log_In()
    {
        const string testUserName = "testuser2";
        const string testPassword = "password";
        Result<User> authenticationResult;

        var testUser = new User((Nickname)testUserName, (FullName)"test1 user1", Password.Create(testPassword).Value, (Email)"testuser1@acme.com", "bio1");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            authenticationResult = await sut.Send(new LogInCommand(testUserName, testPassword), TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            authenticationResult.IsSuccess.Should().BeTrue();
            authenticationResult.Value.Should().NotBeNull();
            authenticationResult.Value.ID.Should().Be(testUser.ID);
        }
    }

    [Fact]
    public async Task Registered_User_Should_Not_Be_Able_To_Log_In_With_Wrong_Password()
    {
        const string testUserName = "testuser3";
        const string testPassword = "password3";
        Result<User> authenticationResult;

        var testUser = new User((Nickname)testUserName, (FullName)"test3 user3", Password.Create(testPassword).Value, (Email)"testuser3@acme.com", "bio3");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            authenticationResult = await sut.Send(new LogInCommand(testUserName, "xxx42"), TestContext.Current.CancellationToken);
        }

        authenticationResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Unregistered_User_Should_Not_Be_Able_To_Log_In()
    {
        Result<User> authenticationResult;

        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session);
            authenticationResult = await sut.Send(new LogInCommand("not_existing_user", "xxx42"), TestContext.Current.CancellationToken);
        }

        authenticationResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Logging_In_And_Out_Should_Raise_Events()
    {
        var testUser = new User((Nickname)"testuser4", (FullName)"test4 user4", Password.Create("password4").Value, (Email)"testuser4@acme.com", "bio4");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var raised = new List<IDomainEvent>();
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, configure: services =>
            {
                services.AddSingleton<INotificationHandler<UserLoggedInEvent>>(new RecordingHandler<UserLoggedInEvent>(raised));
                services.AddSingleton<INotificationHandler<UserLoggedOutEvent>>(new RecordingHandler<UserLoggedOutEvent>(raised));
            });

            (await sut.Send(new LogInCommand("testuser4", "wrong"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
            (await sut.Send(new LogInCommand("testuser4", "password4"), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
            (await sut.Send(new LogOutCommand(testUser.ID), TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        }

        raised.Should().Equal(new UserLoggedInEvent(testUser.ID), new UserLoggedOutEvent(testUser.ID));
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

using System;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Web.CommandHandlers;
using InstaLike.Web.Services;
using Serilog;
using Xunit;

namespace InstaLike.IntegrationTests
{
    public class UserAuthenticationFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;

        public UserAuthenticationFixture(DatabaseFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async void Should_Register_User()
        {
            Result<int> userRegistrationResult;

            var userRegistrationCommand = new RegisterUserCommand(
                "testuser", 
                "test", 
                "user", 
                "password", 
                "testuser@acme.com",
                "test user bio",
                Convert.FromBase64String(_testFixture.GetTestPictureBase64()));
            
            using (var session = _testFixture.OpenSession())
            {
                var handler = new RegisterUserCommandHandler(session, Log.Logger);
                userRegistrationResult = await handler.Handle(userRegistrationCommand, default);
            }

            using (new AssertionScope())
            {
                userRegistrationResult.IsSuccess.Should().BeTrue();
                userRegistrationResult.Value.Should().BeGreaterThan(0);
                using (var session = _testFixture.OpenSession())
                {
                    (await session.QueryOver<User>().RowCountAsync()).Should().Be(1);
                }
            }
        }

        [Fact]
        public async void Registered_User_Should_Be_Able_To_Log_In()
        {
            int testUserID;
            string testUserName = "testuser2";
            string testPassword = "password";
            Result<User> authenticationResult;

            var userRegistrationCommand = new RegisterUserCommand(
                testUserName,
                "test2",
                "user2",
                testPassword,
                "testuser2@acme.com",
                "test user 2 bio",
                Convert.FromBase64String(_testFixture.GetTestPictureBase64()));

            using (var session = _testFixture.OpenSession())
            {
                var handler = new RegisterUserCommandHandler(session, Log.Logger);
                testUserID = (await handler.Handle(userRegistrationCommand, default)).Value;
            }
            
            using (var session = _testFixture.OpenSession())
            {
                var sut = new DatabaseAuthenticationService(_testFixture.SessionFactory, Log.Logger);
                authenticationResult = await sut.AuthenticateUser(testUserName, testPassword);
            }

            using (new AssertionScope())
            {
                authenticationResult.IsSuccess.Should().BeTrue();
                authenticationResult.Value.Should().NotBeNull();
                authenticationResult.Value.ID.Should().Be(testUserID);
            }
        }

        [Fact]
        public async void Registered_User_Should_Not_Be_Able_To_Log_In_With_Wrong_Password()
        {
            string testUserName = "testuser3";
            string testPassword = "password3";
            Result<User> authenticationResult;

            var userRegistrationCommand = new RegisterUserCommand(
                testUserName,
                "test2",
                "user2",
                testPassword,
                "testuser2@acme.com",
                "test user 2 bio",
                Convert.FromBase64String(_testFixture.GetTestPictureBase64()));

            using (var session = _testFixture.OpenSession())
            {
                var handler = new RegisterUserCommandHandler(session, Log.Logger);
                await handler.Handle(userRegistrationCommand, default);
            }

            using (var session = _testFixture.OpenSession())
            {
                var sut = new DatabaseAuthenticationService(_testFixture.SessionFactory, Log.Logger);
                authenticationResult = await sut.AuthenticateUser(testUserName, "xxx42");
            }

            authenticationResult.IsSuccess.Should().BeFalse();
        }
    }
}
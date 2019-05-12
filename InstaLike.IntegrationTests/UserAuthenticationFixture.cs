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
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
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
        public async void Registered_User_Should_Be_Able_To_Log_In()
        {
            string testUserName = "testuser2";
            string testPassword = "password";
            Result<User> authenticationResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test1 user1", Password.Create(testPassword).Value, (Email)"testuser1@acme.com", "bio1");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }
            
            var sut = new DatabaseAuthenticationService(_testFixture.SessionFactory, Log.Logger);
            authenticationResult = await sut.AuthenticateUser(testUserName, testPassword);

            using (new AssertionScope())
            {
                authenticationResult.IsSuccess.Should().BeTrue();
                authenticationResult.Value.Should().NotBeNull();
                authenticationResult.Value.ID.Should().Be(testUser.ID);
            }
        }

        [Fact]
        public async void Registered_User_Should_Not_Be_Able_To_Log_In_With_Wrong_Password()
        {
            string testUserName = "testuser3";
            string testPassword = "password3";
            Result<User> authenticationResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test3 user3", Password.Create(testPassword).Value, (Email)"testuser3@acme.com", "bio3");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }

            var sut = new DatabaseAuthenticationService(_testFixture.SessionFactory, Log.Logger);
            authenticationResult = await sut.AuthenticateUser(testUserName, "xxx42");

            authenticationResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async void Unregistered_User_Should_Not_Be_Able_To_Log_In()
        {
            Result<User> authenticationResult;

            var sut = new DatabaseAuthenticationService(_testFixture.SessionFactory, Log.Logger);
            authenticationResult = await sut.AuthenticateUser("not_existing_user", "xxx42");

            authenticationResult.IsSuccess.Should().BeFalse();
        }
    }
}
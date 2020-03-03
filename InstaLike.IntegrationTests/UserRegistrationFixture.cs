using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Commands;
using InstaLike.Core.Domain;
using InstaLike.Core.Services;
using InstaLike.Web.CommandHandlers;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace InstaLike.IntegrationTests
{
    public class UserRegistrationFixture : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _testFixture;
        private readonly ITestOutputHelper _output;

        public UserRegistrationFixture(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _testFixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Should_Register_User()
        {
            Result<int> userRegistrationResult;

            var userRegistrationCommand = new RegisterUserCommand(
                "registereduser",
                "registered",
                "user",
                "password",
                "registereduser@acme.com",
                "registered user bio",
                Convert.FromBase64String(_testFixture.GetTestPictureBase64()));

            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new RegisterUserCommandHandler(session, Log.Logger, new SequentialGuidGenerator());
                userRegistrationResult = await sut.Handle(userRegistrationCommand, default);
            }

            using (new AssertionScope())
            {
                userRegistrationResult.IsSuccess.Should().BeTrue();
                userRegistrationResult.Value.Should().BeGreaterThan(0);
                using (var session = _testFixture.OpenSession(_output))
                {
                    (await session.GetAsync<User>(userRegistrationResult.Value)).Should().NotBeNull();
                }
            }
        }

        [Fact]
        public async Task Should_Edit_Registered_User_Information()
        {
            const string testUserName = "testuser1";
            const string testPassword = "password";
            Result editCommandResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test1 user1", Password.Create(testPassword).Value, (Email)"testuser1@acme.com", "bio1");

            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }

            var command = new EditUserDetailsCommand(
                testUser.ID, 
                $"{testUser.Nickname}_mod",
                $"{testUser.FullName.Name}_mod",
                $"{testUser.FullName.Surname}_mod",
                "new@email.com",
                "new bio",
                Convert.FromBase64String(_testFixture.GetTestPictureBase64()));
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new EditUserDetailsCommandHandler(session, Log.Logger);
                editCommandResult = await sut.Handle(command, default);
            }

            using (new AssertionScope())
            {
                editCommandResult.IsSuccess.Should().BeTrue();
                using (var session = _testFixture.OpenSession(_output))
                {
                    var modifiedUser = await session.GetAsync<User>(testUser.ID);
                    modifiedUser.Nickname.Should().Be((Nickname)$"{testUser.Nickname}_mod");
                    modifiedUser.FullName.Should().Be((FullName)$"{testUser.FullName.Name}_mod {testUser.FullName.Surname}_mod");
                    modifiedUser.Email.Should().Be((Email)"new@email.com");
                    modifiedUser.Biography.Should().Be("new bio");
                    modifiedUser.ProfilePicture.Should().Be((Picture)_testFixture.GetTestPictureBase64());
                }
            }
        }

        [Fact]
        public async Task Should_Not_Edit_User_Email_If_Email_Is_Not_Valid()
        {
            const string testUserName = "testuser2";
            const string testPassword = "password";
            Result editCommandResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test2 user2", Password.Create(testPassword).Value, (Email)"testuser2@acme.com", "bio2");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }

            var command = new EditUserDetailsCommand(
                testUser.ID,
                testUser.Nickname,
                testUser.FullName.Name,
                testUser.FullName.Surname,
                "xyz",
                testUser.Biography,
                testUser.ProfilePicture);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new EditUserDetailsCommandHandler(session, Log.Logger);
                editCommandResult = await sut.Handle(command, default);
            }

            editCommandResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Not_Edit_User_Nickname_If_Nickname_Is_Not_Valid()
        {
            const string testUserName = "testuser3";
            const string testPassword = "password";
            Result editCommandResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test3 user3", Password.Create(testPassword).Value, (Email)"testuser3@acme.com", "bio3");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }

            var command = new EditUserDetailsCommand(
                testUser.ID,
                string.Empty,
                testUser.FullName.Name,
                testUser.FullName.Surname,
                testUser.Email,
                testUser.Biography,
                testUser.ProfilePicture);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new EditUserDetailsCommandHandler(session, Log.Logger);
                editCommandResult = await sut.Handle(command, default);
            }

            editCommandResult.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Not_Edit_User_Full_Name_If_Full_Name_Is_Not_Valid()
        {
            const string testUserName = "testuser4";
            const string testPassword = "password";
            Result editCommandResult;

            var testUser = new User((Nickname)testUserName, (FullName)"test4 user4", Password.Create(testPassword).Value, (Email)"testuser4@acme.com", "bio4");
            using (var session = _testFixture.OpenSession(_output))
            {
                await session.SaveAsync(testUser);
            }

            var command = new EditUserDetailsCommand(
                testUser.ID,
                testUser.Nickname,
                string.Empty,
                string.Empty,
                testUser.Email,
                testUser.Biography,
                testUser.ProfilePicture);
            using (var session = _testFixture.OpenSession(_output))
            {
                var sut = new EditUserDetailsCommandHandler(session, Log.Logger);
                editCommandResult = await sut.Handle(command, default);
            }

            editCommandResult.IsSuccess.Should().BeFalse();
        }
    }
}

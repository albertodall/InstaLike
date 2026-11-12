using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Accounts.Commands;
using InstaLike.Web.Media;
using Xunit;

namespace InstaLike.IntegrationTests;

public class UserRegistrationFixture : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _testFixture;
    private readonly ITestOutputHelper _output;
    private readonly InMemoryPictureStore _pictureStore = new();

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
            Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));

        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            userRegistrationResult = await sut.Send(userRegistrationCommand, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            userRegistrationResult.IsSuccess.Should().BeTrue();
            userRegistrationResult.Value.Should().BeGreaterThan(0);
            using (var session = _testFixture.OpenSession(_output))
            {
                var registeredUser = await session.GetAsync<User>(userRegistrationResult.Value, TestContext.Current.CancellationToken);
                registeredUser.Should().NotBeNull();
                _pictureStore.GetContent(PictureContainers.Profiles, registeredUser.ProfilePicture!).Should().Equal(userRegistrationCommand.ProfilePicture);
            }
        }
    }

    [Fact]
    public async Task Should_Register_User_Without_A_Profile_Picture()
    {
        var command = new RegisterUserCommand("nopictureuser", "nopicture", "user", "password", "nopictureuser@acme.com", "bio", null);

        Result<int> registrationResult;
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            registrationResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        registrationResult.IsSuccess.Should().BeTrue();
        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.GetAsync<User>(registrationResult.Value, TestContext.Current.CancellationToken)).ProfilePicture.Should().BeNull();
        }
    }

    [Fact]
    public async Task Should_Not_Register_A_User_With_An_Email_Already_In_Use()
    {
        var existingUser = TestDataHelper.NewUser("emailowner", "taken@acme.com");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(existingUser, TestContext.Current.CancellationToken);
        }

        var command = new RegisterUserCommand("emailthief", "email", "thief", "password", "Taken@Acme.com", "bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var result = await DatabaseFixture.CreateMediator(session, _pictureStore).Send(command, TestContext.Current.CancellationToken);
            result.Error.Should().Be("This email is already used by another account.");
        }
    }

    [Fact]
    public async Task Should_Not_Register_A_User_With_A_Nickname_Already_Taken()
    {
        var existingUser = TestDataHelper.NewUser("takennick", "nickowner@acme.com");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(existingUser, TestContext.Current.CancellationToken);
        }

        var command = new RegisterUserCommand("takennick", "nick", "thief", "password", "nickthief@acme.com", "bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var result = await DatabaseFixture.CreateMediator(session, _pictureStore).Send(command, TestContext.Current.CancellationToken);
            result.Error.Should().Be("This nickname is already taken.");
        }
    }

    [Fact]
    public async Task Should_Not_Change_The_Email_To_One_Used_By_Another_Account()
    {
        var firstUser = TestDataHelper.NewUser("firstowner", "first@acme.com");
        var secondUser = TestDataHelper.NewUser("secondowner", "second@acme.com");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(firstUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(secondUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(secondUser.ID, secondUser.Nickname, secondUser.FullName.Name, secondUser.FullName.Surname, "FIRST@acme.com", secondUser.Biography, null);
        using (var session = _testFixture.OpenSession(_output))
        {
            (await DatabaseFixture.CreateMediator(session, _pictureStore).Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeFalse();
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.GetAsync<User>(secondUser.ID, TestContext.Current.CancellationToken)).Email.Value.Should().Be("second@acme.com");
        }
    }

    [Fact]
    public async Task Should_Edit_Registered_User_Information()
    {
        const string testUserName = "testuser1";
        Result editCommandResult;

        var testUser = TestDataHelper.NewUser(testUserName);

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(
            testUser.ID,
            $"{testUser.Nickname}_mod",
            $"{testUser.FullName.Name}_mod",
            $"{testUser.FullName.Surname}_mod",
            "new@email.com",
            "new bio",
            Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            editCommandResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                var modifiedUser = await session.GetAsync<User>(testUser.ID, TestContext.Current.CancellationToken);
                modifiedUser.Nickname.Value.Should().Be($"{testUser.Nickname}_mod");
                modifiedUser.FullName.Name.Should().Be($"{testUser.FullName.Name}_mod");
                modifiedUser.FullName.Surname.Should().Be($"{testUser.FullName.Surname}_mod");
                modifiedUser.Email.Value.Should().Be("new@email.com");
                modifiedUser.Biography.Should().Be("new bio");
                _pictureStore.GetContent(PictureContainers.Profiles, modifiedUser.ProfilePicture!).Should().Equal(Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
            }
        }
    }

    [Fact]
    public async Task Should_Give_Each_New_Profile_Picture_Its_Own_Identifier()
    {
        var firstUser = TestDataHelper.NewUser("testuser5");
        var secondUser = TestDataHelper.NewUser("testuser6");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(firstUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(secondUser, TestContext.Current.CancellationToken);
        }

        var picture = Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64());
        foreach (var user in new[] { firstUser, secondUser })
        {
            var command = new EditUserDetailsCommand(user.ID, user.Nickname, user.FullName.Name, user.FullName.Surname, user.Email, user.Biography, picture);
            using var session = _testFixture.OpenSession(_output);
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            (await sut.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var firstPicture = (await session.GetAsync<User>(firstUser.ID, TestContext.Current.CancellationToken)).ProfilePicture;
            var secondPicture = (await session.GetAsync<User>(secondUser.ID, TestContext.Current.CancellationToken)).ProfilePicture;
            firstPicture.Should().NotBe(secondPicture);
        }
    }

    [Fact]
    public async Task Should_Keep_The_Profile_Picture_When_No_New_Picture_Is_Uploaded()
    {
        var testUser = TestDataHelper.NewUser("testuser7");
        testUser.SetProfilePicture(PictureId.New());
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(testUser.ID, testUser.Nickname, testUser.FullName.Name, testUser.FullName.Surname, testUser.Email, "new bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            (await sut.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.GetAsync<User>(testUser.ID, TestContext.Current.CancellationToken)).ProfilePicture.Should().Be(testUser.ProfilePicture);
        }
    }

    [Fact]
    public async Task Should_Delete_The_Replaced_Profile_Picture()
    {
        var picture = Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64());
        var oldPicture = await _pictureStore.SaveAsync(PictureContainers.Profiles, picture, TestContext.Current.CancellationToken);
        var testUser = TestDataHelper.NewUser("testuser8");
        testUser.SetProfilePicture(oldPicture);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(testUser.ID, testUser.Nickname, testUser.FullName.Name, testUser.FullName.Surname, testUser.Email, testUser.Biography, picture);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            (await sut.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        }

        _pictureStore.GetContent(PictureContainers.Profiles, oldPicture).Should().BeNull();
    }

    [Fact]
    public async Task Should_Not_Edit_User_Email_If_Email_Is_Not_Valid()
    {
        const string testUserName = "testuser2";
        Result editCommandResult;

        var testUser = TestDataHelper.NewUser(testUserName);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(
            testUser.ID,
            testUser.Nickname,
            testUser.FullName.Name,
            testUser.FullName.Surname,
            "xyz",
            testUser.Biography,
            null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        editCommandResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Not_Edit_User_Nickname_If_Nickname_Is_Not_Valid()
    {
        const string testUserName = "testuser3";
        Result editCommandResult;

        var testUser = TestDataHelper.NewUser(testUserName);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(
            testUser.ID,
            string.Empty,
            testUser.FullName.Name,
            testUser.FullName.Surname,
            testUser.Email,
            testUser.Biography,
            null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        editCommandResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Not_Edit_User_Full_Name_If_Full_Name_Is_Not_Valid()
    {
        const string testUserName = "testuser4";
        Result editCommandResult;

        var testUser = TestDataHelper.NewUser(testUserName);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
        }

        var command = new EditUserDetailsCommand(
            testUser.ID,
            testUser.Nickname,
            string.Empty,
            string.Empty,
            testUser.Email,
            testUser.Biography,
            null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = DatabaseFixture.CreateMediator(session, _pictureStore);
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        editCommandResult.IsSuccess.Should().BeFalse();
    }
}

using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using InstaLike.Web.Membership.Commands;
using InstaLike.Web.Media;
using Xunit;

namespace InstaLike.IntegrationTests;

public sealed class MemberRegistrationFixture : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _testFixture;
    private readonly ITestOutputHelper _output;
    private readonly InMemoryPictureStore _pictureStore = new();

    public MemberRegistrationFixture(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _testFixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Should_Register_A_Member_With_Its_Account()
    {
        Result<MemberId> userRegistrationResult;

        var userRegistrationCommand = new RegisterMemberCommand(
            "registereduser",
            "registered",
            "user",
            "password",
            "registereduser@acme.com",
            "registered user bio",
            Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));

        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            userRegistrationResult = await sut.Send(userRegistrationCommand, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            userRegistrationResult.IsSuccess.Should().BeTrue();
            userRegistrationResult.Value.Value.Should().NotBeEmpty();
            using (var session = _testFixture.OpenSession(_output))
            {
                var registeredUser = await session.GetAsync<Member>(userRegistrationResult.Value, TestContext.Current.CancellationToken);
                registeredUser.Should().NotBeNull();
                _pictureStore.GetContent(PictureContainers.Profiles, registeredUser.ProfilePicture!).Should().Equal(userRegistrationCommand.ProfilePicture);

                var account = await session.GetAsync<Account>(AccountId.From(userRegistrationResult.Value.Value), TestContext.Current.CancellationToken);
                account.Should().NotBeNull();
                account.Username.Value.Should().Be("registereduser");
                account.Email.Value.Should().Be("registereduser@acme.com");
                account.Password.HashMatches("password").Should().BeTrue();
            }
        }
    }

    [Fact]
    public async Task Should_Register_A_Member_Without_A_Profile_Picture()
    {
        var command = new RegisterMemberCommand("nopictureuser", "nopicture", "user", "password", "nopictureuser@acme.com", "bio", null);

        Result<MemberId> registrationResult;
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            registrationResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        registrationResult.IsSuccess.Should().BeTrue();
        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.GetAsync<Member>(registrationResult.Value, TestContext.Current.CancellationToken)).ProfilePicture.Should().BeNull();
        }
    }

    [Fact]
    public async Task Should_Not_Register_A_Member_With_An_Email_Already_In_Use()
    {
        var existingMember = TestDataHelper.NewMember("emailowner");
        await SaveAsync(existingMember, TestDataHelper.NewAccountWithEmail(existingMember, "taken@acme.com"));

        var command = new RegisterMemberCommand("emailthief", "email", "thief", "password", "Taken@Acme.com", "bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var result = await new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build().Send(command, TestContext.Current.CancellationToken);
            result.Error.Should().Be("This email is already used by another account.");
        }

        (await CountMembersAsync("emailthief")).Should().Be(0);
    }

    [Fact]
    public async Task Should_Not_Register_A_Member_With_A_Nickname_Already_Taken()
    {
        var existingMember = TestDataHelper.NewMember("takennick");
        await SaveAsync(existingMember, TestDataHelper.NewAccount(existingMember));

        var command = new RegisterMemberCommand("takennick", "nick", "thief", "password", "nickthief@acme.com", "bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var result = await new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build().Send(command, TestContext.Current.CancellationToken);
            result.Error.Should().Be("This nickname is already taken.");
        }
    }

    [Fact]
    public async Task Should_Not_Register_A_Member_Whose_Nickname_Is_Still_Another_Account_Username()
    {
        var existingMember = TestDataHelper.NewMember("oldname");
        await SaveAsync(existingMember, TestDataHelper.NewAccount(existingMember));
        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.GetAsync<Member>(existingMember.ID, TestContext.Current.CancellationToken)).ChangeNickname((Nickname)"newname");
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new RegisterMemberCommand("oldname", "old", "name", "password", "oldname-thief@acme.com", "bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var result = await new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build().Send(command, TestContext.Current.CancellationToken);
            result.Error.Should().Be("This nickname is already taken.");
        }

        (await CountMembersAsync("oldname")).Should().Be(0);
    }

    [Fact]
    public async Task Should_Edit_The_Member_Profile()
    {
        const string testUserName = "testuser1";
        Result editCommandResult;

        var testUser = TestDataHelper.NewMember(testUserName);

        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditMemberProfileCommand(
            testUser.ID,
            $"{testUser.Nickname}_mod",
            $"{testUser.FullName.Name}_mod",
            $"{testUser.FullName.Surname}_mod",
            "new bio",
            Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        using (new AssertionScope())
        {
            editCommandResult.IsSuccess.Should().BeTrue();
            using (var session = _testFixture.OpenSession(_output))
            {
                var modifiedUser = await session.GetAsync<Member>(testUser.ID, TestContext.Current.CancellationToken);
                modifiedUser.Nickname.Value.Should().Be($"{testUser.Nickname}_mod");
                modifiedUser.FullName.Name.Should().Be($"{testUser.FullName.Name}_mod");
                modifiedUser.FullName.Surname.Should().Be($"{testUser.FullName.Surname}_mod");
                modifiedUser.Biography.Should().Be("new bio");
                _pictureStore.GetContent(PictureContainers.Profiles, modifiedUser.ProfilePicture!).Should().Equal(Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64()));
            }
        }
    }

    [Fact]
    public async Task Should_Give_Each_New_Profile_Picture_Its_Own_Identifier()
    {
        var firstUser = TestDataHelper.NewMember("testuser5");
        var secondUser = TestDataHelper.NewMember("testuser6");
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(firstUser, TestContext.Current.CancellationToken);
            await session.SaveAsync(secondUser, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var picture = Convert.FromBase64String(DatabaseFixture.GetTestPictureBase64());
        foreach (var user in new[] { firstUser, secondUser })
        {
            var command = new EditMemberProfileCommand(user.ID, user.Nickname, user.FullName.Name, user.FullName.Surname, user.Biography, picture);
            using var session = _testFixture.OpenSession(_output);
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            (await sut.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            var firstPicture = (await session.GetAsync<Member>(firstUser.ID, TestContext.Current.CancellationToken)).ProfilePicture;
            var secondPicture = (await session.GetAsync<Member>(secondUser.ID, TestContext.Current.CancellationToken)).ProfilePicture;
            firstPicture.Should().NotBe(secondPicture);
        }
    }

    [Fact]
    public async Task Should_Keep_The_Profile_Picture_When_No_New_Picture_Is_Uploaded()
    {
        var testUser = TestDataHelper.NewMember("testuser7");
        testUser.SetProfilePicture(PictureId.New());
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditMemberProfileCommand(testUser.ID, testUser.Nickname, testUser.FullName.Name, testUser.FullName.Surname, "new bio", null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            (await sut.Send(command, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        }

        using (var session = _testFixture.OpenSession(_output))
        {
            (await session.GetAsync<Member>(testUser.ID, TestContext.Current.CancellationToken)).ProfilePicture.Should().Be(testUser.ProfilePicture);
        }
    }

    [Fact]
    public async Task Should_Not_Edit_The_Member_Nickname_If_Nickname_Is_Not_Valid()
    {
        const string testUserName = "testuser3";
        Result editCommandResult;

        var testUser = TestDataHelper.NewMember(testUserName);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditMemberProfileCommand(
            testUser.ID,
            string.Empty,
            testUser.FullName.Name,
            testUser.FullName.Surname,
            testUser.Biography,
            null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        editCommandResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Not_Edit_The_Member_Full_Name_If_Full_Name_Is_Not_Valid()
    {
        const string testUserName = "testuser4";
        Result editCommandResult;

        var testUser = TestDataHelper.NewMember(testUserName);
        using (var session = _testFixture.OpenSession(_output))
        {
            await session.SaveAsync(testUser, TestContext.Current.CancellationToken);
            await session.FlushAsync(TestContext.Current.CancellationToken);
        }

        var command = new EditMemberProfileCommand(
            testUser.ID,
            testUser.Nickname,
            string.Empty,
            string.Empty,
            testUser.Biography,
            null);
        using (var session = _testFixture.OpenSession(_output))
        {
            var sut = new TestMediatorBuilder(session).WithPictureStore(_pictureStore).Build();
            editCommandResult = await sut.Send(command, TestContext.Current.CancellationToken);
        }

        editCommandResult.IsSuccess.Should().BeFalse();
    }

    private async Task SaveAsync(Member member, Account account)
    {
        using var session = _testFixture.OpenSession(_output);
        await session.SaveAsync(member, TestContext.Current.CancellationToken);
        await session.SaveAsync(account, TestContext.Current.CancellationToken);
        await session.FlushAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> CountMembersAsync(string nickname)
    {
        using var session = _testFixture.OpenStatelessSession();
        return await session.QueryOver<Member>()
            .Where(NHibernate.Criterion.Restrictions.Eq("Nickname", nickname))
            .RowCountAsync(TestContext.Current.CancellationToken);
    }
}

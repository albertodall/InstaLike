using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests;

public sealed class UserFixture
{
    [Fact]
    public void New_User_Should_Have_No_Profile_Picture()
    {
        var sut = CreateTestUser();

        sut.ProfilePicture.Should().BeNull();
    }

    [Fact]
    public void User_Should_Change_Nickname()
    {
        var sut = CreateTestUser();
        var newNickname = Nickname.Create("newnickname");

        sut.ChangeNickname(newNickname.Value);

        sut.Nickname.Value.Should().Be(newNickname.Value);
    }

    [Fact]
    public void User_Should_Change_Full_Name()
    {
        var sut = CreateTestUser();
        var newFullName = (FullName)"Test User New";

        sut.ChangeFullName(newFullName);

        sut.FullName.Should().Be(newFullName);
    }

    [Fact]
    public void User_Should_Change_Email_Address()
    {
        var sut = CreateTestUser();
        var newEmailAddress = (Email)"user1@newdomain.com";

        sut.ChangeEmailAddress(newEmailAddress);

        sut.Email.Should().Be(newEmailAddress);
    }

    [Fact]
    public void User_Should_Update_Biography()
    {
        var sut = CreateTestUser();
        var newBiographyText = "This is my new bio";

        sut.UpdateBiography(newBiographyText);

        sut.Biography.Should().Be(newBiographyText);
    }

    [Fact]
    public void Should_Change_User_Password()
    {
        var password1 = Password.Create("password1").Value;
        var password2 = Password.Create("password2").Value;
        var sut = new User(
            (Nickname)"user1",
            (FullName)"Test User",
            password1,
            (Email)"user1@acme.com",
            "My Bio");

        sut.ChangePassword(password2);

        sut.Password.HashMatches(password1).Should().BeFalse();
    }

    [Fact]
    public void Should_Set_New_Profile_Picture()
    {
        var newProfilePicture = PictureId.New();
        var sut = CreateTestUser();

        sut.SetProfilePicture(newProfilePicture);

        sut.ProfilePicture.Should().Be(newProfilePicture);
    }

    [Fact]
    public void Should_Remove_Profile_Picture()
    {
        var newProfilePicture = PictureId.New();
        var sut = CreateTestUser();

        sut.SetProfilePicture(newProfilePicture);
        sut.RemoveProfilePicture();

        sut.ProfilePicture.Should().BeNull();
    }

    [Fact]
    public void Logging_In_With_The_Right_Password_Should_Raise_An_Event()
    {
        var sut = CreateUserWithPassword("password");

        using (new AssertionScope())
        {
            sut.LogIn("password").IsSuccess.Should().BeTrue();
            sut.DomainEvents.Should().ContainSingle().Which.Should().Be(new UserLoggedInEvent(sut.ID));
        }
    }

    [Fact]
    public void Logging_In_With_A_Wrong_Password_Should_Fail_Without_Events()
    {
        var sut = CreateUserWithPassword("password");

        using (new AssertionScope())
        {
            sut.LogIn("wrong").IsSuccess.Should().BeFalse();
            sut.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Logging_Out_Should_Raise_An_Event()
    {
        var sut = CreateUserWithPassword("password");

        sut.LogOut();

        sut.DomainEvents.Should().ContainSingle().Which.Should().Be(new UserLoggedOutEvent(sut.ID));
    }

    private static User CreateUserWithPassword(string password)
    {
        return new User((Nickname)"user1", (FullName)"Test User", Password.Create(password).Value, (Email)"user1@acme.com", "My Bio");
    }

    private static User CreateTestUser()
    {
        return new User(
            (Nickname)"user1",
            (FullName)"Test User",
            (Password)"password",
            (Email)"user1@acme.com",
            "My Bio");
    }
}

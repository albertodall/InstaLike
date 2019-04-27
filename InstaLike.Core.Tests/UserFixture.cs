using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class UserFixture
    {
        [Fact]
        public void New_User_Should_Have_Default_Profile_Picture()
        {
            var sut = new User(
                (Nickname)"user1",
                (FullName)"Test User",
                Password.Create("password1").Value,
                (Email)"user1@acme.com",
                "My Bio");

            sut.ProfilePicture.RawBytes.Should().Equal(Picture.DefaultProfilePicture.RawBytes);
        }

        [Fact]
        public void User_Should_Change_Nickname()
        {
            var sut = new User(
                (Nickname)"user1",
                (FullName)"Test User",
                Password.Create("password1").Value,
                (Email)"user1@acme.com",
                "My Bio");

            var newNickname = Nickname.Create("newnickname");
            sut.ChangeNickname(newNickname.Value);

            sut.Nickname.Value.Should().Be(newNickname.Value);
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
        public void User_Should_Unfollow_Another_User()
        {
            var sut = new User(
                (Nickname)"user1",
                (FullName)"Test User 1",
                Password.Create("password").Value,
                (Email)"user1@acme.com",
                "My Bio");

            var followedUser = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                Password.Create("password").Value,
                (Email)"user2@acme.com",
                "My Bio");

            sut.Follow(followedUser);

            sut.Unfollow(followedUser);
            sut.IsFollowing(followedUser).Should().BeFalse();
            sut.Followed.Count.Should().Be(0);
            followedUser.Followers.Count.Should().Be(0);
        }
    }
}

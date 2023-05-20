using System;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Domain;
using InstaLike.Core.Tests.Properties;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class UserFixture
    {
        private readonly string _testPictureBase64 = Convert.ToBase64String(Resources.GrumpyCat);

        [Fact]
        public void New_User_Should_Have_Default_Profile_Picture()
        {
            var sut = CreateTestUser();

            sut.ProfilePicture.Should().Be(Picture.DefaultProfilePicture);
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
            var newProfilePicture = (Picture)_testPictureBase64;
            var sut = CreateTestUser();

            sut.SetProfilePicture(newProfilePicture);

            sut.ProfilePicture.Should().Be(newProfilePicture);
        }

        [Fact]
        public void Should_Set_Default_Profile_Picture()
        {
            var newProfilePicture = (Picture)_testPictureBase64;
            var sut = CreateTestUser();

            sut.SetProfilePicture(newProfilePicture);
            sut.SetDefaultProfilePicture();

            sut.ProfilePicture.Should().Be(Picture.DefaultProfilePicture);
        }

        [Fact]
        public void User_Should_Follow_Another_User()
        {
            var sut = CreateTestUser();
            var followedUser = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");

            sut.Follow(followedUser);

            using (new AssertionScope())
            {
                sut.IsFollowing(followedUser).Should().BeTrue();
                sut.Followed.Count.Should().Be(1);
                followedUser.IsFollowedBy(sut).Should().BeTrue();
                followedUser.Followers.Count.Should().Be(1);
            }
        }

        [Fact]
        public void User_Should_Unfollow_Another_User()
        {
            var sut = CreateTestUser();
            var followedUser = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");

            sut.Follow(followedUser);
            sut.Unfollow(followedUser);

            using (new AssertionScope())
            {
                sut.IsFollowing(followedUser).Should().BeFalse();
                sut.Followed.Count.Should().Be(0);
                followedUser.IsFollowedBy(sut).Should().BeFalse();
                followedUser.Followers.Count.Should().Be(0);
            }
        }

        [Fact]
        public void Should_Not_Unfollow_Unfollowed_User()
        {
            var sut = CreateTestUser();
            var otherUser = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");

            sut.Unfollow(otherUser).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Cannot_Follow_User_Twice()
        {
            var sut = CreateTestUser();
            var otherUser = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");

            using (new AssertionScope())
            {
                sut.Follow(otherUser).IsSuccess.Should().BeTrue();
                sut.Follow(otherUser).IsSuccess.Should().BeFalse();
            }
        }

        [Fact]
        public void Should_Put_A_Like_On_A_Post()
        {
            var sut = CreateTestUser();
            var postAuthor = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var post = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            using (new AssertionScope())
            {
                sut.PutLikeTo(post).IsSuccess.Should().BeTrue();
                post.LikesTo(sut).Should().BeTrue();
            }
        }

        [Fact]
        public void Should_Not_Put_Like_Twice()
        {
            var sut = CreateTestUser();
            var postAuthor = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var post = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            using (new AssertionScope())
            {
                sut.PutLikeTo(post).IsSuccess.Should().BeTrue();
                sut.PutLikeTo(post).IsSuccess.Should().BeFalse();
            }
        }

        [Fact]
        public void Should_Not_Put_A_Like_On_Own_Post()
        {
            var sut = CreateTestUser();
            var post = new Post(sut, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeTo(post).IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Should_Remove_A_Like_To_A_Post()
        {
            var sut = CreateTestUser();
            var postAuthor = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var post = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeTo(post);

            sut.RemoveLikeFrom(post).IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Remove_Like_Twice()
        {
            var sut = CreateTestUser();
            var postAuthor = new User(
                (Nickname)"user2",
                (FullName)"Test User 2",
                (Password)"password",
                (Email)"user2@acme.com",
                "My Bio");
            var post = new Post(postAuthor, (Picture)_testPictureBase64, (PostText)"test post");

            sut.PutLikeTo(post);

            using (new AssertionScope())
            {
                sut.RemoveLikeFrom(post).IsSuccess.Should().BeTrue();
                sut.RemoveLikeFrom(post).IsSuccess.Should().BeFalse();
            }
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
}
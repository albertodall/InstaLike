using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Core.SharedKernel;
using Xunit;

namespace InstaLike.Core.Tests
{
    public sealed class MemberFixture
    {
        [Fact]
        public void A_New_Member_Should_Be_Identified_Before_It_Is_Stored()
        {
            var sut = CreateTestMember();

            sut.ID.Value.Should().NotBeEmpty();
        }

        [Fact]
        public void New_Member_Should_Have_No_Profile_Picture()
        {
            var sut = CreateTestMember();

            sut.ProfilePicture.Should().BeNull();
        }

        [Fact]
        public void Member_Should_Change_Nickname()
        {
            var sut = CreateTestMember();
            var newNickname = Nickname.Create("newnickname");

            sut.ChangeNickname(newNickname.Value);

            sut.Nickname.Value.Should().Be(newNickname.Value);
        }

        [Fact]
        public void Member_Should_Change_Full_Name()
        {
            var sut = CreateTestMember();
            var newFullName = (FullName)"Test User New";

            sut.ChangeFullName(newFullName);

            sut.FullName.Should().Be(newFullName);
        }

        [Fact]
        public void Member_Should_Update_Biography()
        {
            var sut = CreateTestMember();
            var newBiographyText = "This is my new bio";

            sut.UpdateBiography(newBiographyText);

            sut.Biography.Should().Be(newBiographyText);
        }

        [Fact]
        public void Should_Set_New_Profile_Picture()
        {
            var newProfilePicture = PictureId.New();
            var sut = CreateTestMember();

            sut.SetProfilePicture(newProfilePicture);

            sut.ProfilePicture.Should().Be(newProfilePicture);
        }

        [Fact]
        public void Should_Remove_Profile_Picture()
        {
            var newProfilePicture = PictureId.New();
            var sut = CreateTestMember();

            sut.SetProfilePicture(newProfilePicture);
            sut.RemoveProfilePicture();

            sut.ProfilePicture.Should().BeNull();
        }

        [Fact]
        public void Registering_Should_Pass_The_Credentials_To_The_Account_Without_Keeping_Them()
        {
            var username = (Username)"user1";
            var password = Password.Create("password").Value;
            var email = (Email)"user1@acme.com";

            var sut = Member.Register((Nickname)"user1", (FullName)"Test User", "My Bio", username, password, email);

            sut.DomainEvents.Should().ContainSingle()
                .Which.Should().Be(new MemberRegisteredEvent(sut.ID, username, password, email));
        }

        private static Member CreateTestMember()
        {
            return Member.Register(
                (Nickname)"user1",
                (FullName)"Test User",
                "My Bio",
                (Username)"user1",
                Password.Create("password").Value,
                (Email)"user1@acme.com");
        }
    }
}

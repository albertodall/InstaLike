using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    public class UserFixture
    {
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
    }
}

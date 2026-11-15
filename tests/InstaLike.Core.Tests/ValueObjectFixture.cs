using FluentAssertions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using Xunit;

namespace InstaLike.Core.Tests
{
    public sealed class ValueObjectFixture
    {
        [Fact]
        public void Value_Objects_Of_Different_Types_With_The_Same_Value_Should_Differ()
        {
            var nickname = Nickname.Create("alby").Value;
            var username = Username.Create("alby").Value;

            nickname.Equals(username).Should().BeFalse();
        }

        [Fact]
        public void A_Single_Value_Object_Should_Print_Its_Value()
        {
            Nickname.Create("alby").Value.ToString().Should().Be("alby");
        }
    }
}

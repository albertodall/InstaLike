using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Social;
using Xunit;

namespace InstaLike.Core.Tests;

public class FollowFixture
{
    [Fact]
    public void User_Should_Follow_Another_User()
    {
        var result = Follow.Create(1, 2);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.FollowerID.Should().Be(1);
            result.Value.FollowedID.Should().Be(2);
        }
    }

    [Fact]
    public void Following_Should_Raise_An_Event()
    {
        var follow = Follow.Create(1, 2).Value;

        follow.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(new FollowedUserEvent(1, 2));
    }

    [Fact]
    public void User_Cannot_Follow_Themselves()
    {
        Follow.Create(1, 1).IsSuccess.Should().BeFalse();
    }
}

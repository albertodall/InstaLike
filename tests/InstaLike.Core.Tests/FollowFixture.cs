using System;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Social;
using Xunit;

namespace InstaLike.Core.Tests;

public sealed class FollowFixture
{
    private static readonly Guid FollowerID = Guid.NewGuid();
    private static readonly Guid FollowedID = Guid.NewGuid();

    [Fact]
    public void User_Should_Follow_Another_User()
    {
        var result = Follow.Create(FollowerID, FollowedID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.FollowerID.Should().Be(FollowerID);
            result.Value.FollowedID.Should().Be(FollowedID);
        }
    }

    [Fact]
    public void Following_Should_Raise_An_Event()
    {
        var follow = Follow.Create(FollowerID, FollowedID).Value;

        follow.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(new FollowedUserEvent(FollowerID, FollowedID));
    }

    [Fact]
    public void User_Cannot_Follow_Themselves()
    {
        Follow.Create(FollowerID, FollowerID).IsSuccess.Should().BeFalse();
    }
}

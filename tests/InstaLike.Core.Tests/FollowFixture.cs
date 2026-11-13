using System;
using FluentAssertions;
using FluentAssertions.Execution;
using InstaLike.Core.Social;
using Xunit;

namespace InstaLike.Core.Tests;

public sealed class FollowFixture
{
    private static readonly Guid MemberID = Guid.NewGuid();
    private static readonly Guid FollowedID = Guid.NewGuid();

    [Fact]
    public void Member_Should_Follow_Another_Member()
    {
        var follower = new Follower(MemberID);

        var result = follower.Follow(FollowedID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            follower.ID.Should().Be(FollowerId.From(MemberID));
            follower.IsFollowing(FollowedID).Should().BeTrue();
            follower.Follows.Should().ContainSingle()
                .Which.Follower.Should().BeSameAs(follower);
        }
    }

    [Fact]
    public void Following_Should_Raise_An_Event()
    {
        var follower = new Follower(MemberID);

        follower.Follow(FollowedID);

        follower.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(new FollowedUserEvent(MemberID, FollowedID));
    }

    [Fact]
    public void Member_Cannot_Follow_Themselves()
    {
        var follower = new Follower(MemberID);

        var result = follower.Follow(MemberID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeFalse();
            follower.Follows.Should().BeEmpty();
            follower.DomainEvents.Should().BeEmpty();
        }
    }

    [Fact]
    public void Member_Cannot_Follow_The_Same_Member_Twice()
    {
        var follower = new Follower(MemberID);
        follower.Follow(FollowedID);

        var result = follower.Follow(FollowedID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeFalse();
            follower.Follows.Should().ContainSingle();
            follower.DomainEvents.Should().ContainSingle();
        }
    }

    [Fact]
    public void Member_Should_Unfollow_A_Followed_Member()
    {
        var follower = new Follower(MemberID);
        follower.Follow(FollowedID);

        var result = follower.Unfollow(FollowedID);

        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            follower.IsFollowing(FollowedID).Should().BeFalse();
        }
    }

    [Fact]
    public void Member_Cannot_Unfollow_A_Member_They_Do_Not_Follow()
    {
        var follower = new Follower(MemberID);

        follower.Unfollow(FollowedID).IsSuccess.Should().BeFalse();
    }
}

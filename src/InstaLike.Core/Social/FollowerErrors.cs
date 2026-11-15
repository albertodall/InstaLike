using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Social
{
    public static class FollowerErrors
    {
        public static readonly Error SelfFollow =
            Error.Failure("Follower.SelfFollow", "You cannot follow yourself.");

        public static readonly Error AlreadyFollowing =
            Error.Conflict("Follower.AlreadyFollowing", "You are already following this member.");

        public static readonly Error NotFollowing =
            Error.Failure("Follower.NotFollowing", "You are not following this member.");
    }
}

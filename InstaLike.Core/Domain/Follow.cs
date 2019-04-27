using System;

namespace InstaLike.Core.Domain
{
    public class Follow : EntityBase<int>
    {
        protected Follow()
        { }

        public Follow(User follower, User followed)
        {
            Follower = follower ?? throw new ArgumentNullException(nameof(follower));
            Followed = followed ?? throw new ArgumentNullException(nameof(followed));
            FollowDate = DateTimeOffset.Now;
        }

        public virtual User Follower { get; protected set; }
        public virtual User Followed { get; protected set; }
        public virtual DateTimeOffset FollowDate { get; protected set; }
    }
}

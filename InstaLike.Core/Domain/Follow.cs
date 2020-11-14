using System;

namespace InstaLike.Core.Domain
{
    public class Follow : EntityBase<int>
    {
        protected Follow() { }

        public Follow(User follower, User followed) 
            : this()
        {
            Follower = follower ?? throw new ArgumentNullException(nameof(follower));
            Followed = followed ?? throw new ArgumentNullException(nameof(followed));
            FollowDate = DateTimeOffset.Now;
        }

        public virtual User Follower { get; }
        public virtual User Followed { get; }
        public virtual DateTimeOffset FollowDate { get; }
    }
}

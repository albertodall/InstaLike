using System;
using System.Collections.Generic;
using System.Text;

namespace InstaLike.Core.Domain
{
    public class Follow : EntityBase<int>
    {
        protected Follow()
        { }

        public Follow(User follower, User following)
        {
            Follower = follower ?? throw new ArgumentNullException(nameof(follower));
            Following = following ?? throw new ArgumentNullException(nameof(following));
            FollowDate = DateTimeOffset.Now;
        }

        public virtual User Follower { get; protected set; }
        public virtual User Following { get; protected set; }
        public virtual DateTimeOffset FollowDate { get; protected set; }
    }
}

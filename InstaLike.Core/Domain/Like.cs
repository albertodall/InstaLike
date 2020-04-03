using System;

namespace InstaLike.Core.Domain
{
    public class Like : EntityBase<int>
    {
        protected Like() { }

        public Like(Post post, User user) : this()
        {
            Post = post;
            User = user;
            LikeDate = DateTimeOffset.Now;
        }

        public virtual Post Post { get; }

        public virtual User User { get; }

        public virtual DateTimeOffset LikeDate { get; }
    }
}
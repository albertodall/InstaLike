using System;

namespace InstaLike.Core.Domain
{
    public class Like : EntityBase<int>
    {
#pragma warning disable CS8618
        protected Like() { }
#pragma warning restore CS8618

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
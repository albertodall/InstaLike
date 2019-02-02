using System;

namespace InstaLike.Core.Domain
{
    public class Like : EntityBase<int>
    {
        protected Like()
        { }

        public Like(Post post, User user)
        {
            Post = post;
            User = user;
            Date = DateTimeOffset.Now;
        }

        public virtual Post Post { get; protected set; }

        public virtual User User { get; protected set; }

        public virtual DateTimeOffset Date { get; protected set; }
    }
}
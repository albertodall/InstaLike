using System;

namespace InstaLike.Core.Domain
{
    public class Comment : EntityBase<int>
    {
        protected Comment()
        { }

        public Comment(Post post, User user, string text)
        {
            Post = post;
            User = User;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public virtual Post Post { get; }
        public virtual User User { get; }
        public virtual string Text { get; }
    }
}
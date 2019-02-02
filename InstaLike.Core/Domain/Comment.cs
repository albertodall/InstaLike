using System;

namespace InstaLike.Core.Domain
{
    public class Comment : EntityBase<int>
    {
        protected Comment()
        { }

        public Comment(Post post, User user, string commentText)
        {
            Post = post;
            User = User;
            CommentText = commentText ?? throw new ArgumentNullException(nameof(commentText));
        }

        public virtual Post Post { get; }
        public virtual User User { get; }
        public virtual string CommentText { get; }
    }
}
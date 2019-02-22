using System;
using System.Collections.Generic;
using System.Linq;

namespace InstaLike.Core.Domain
{
    public class Post : EntityBase<int>
    {
        private readonly IList<Comment> _comments;
        private readonly IList<Like> _likes;

        protected Post()
        {
            _comments = new List<Comment>();
            _likes = new List<Like>();
        }

        public Post(User author, Picture picture, string comment)
            : this()
        {
            Author = author;
            Picture = picture;
            Text = comment;

            PostDate = DateTimeOffset.Now;
        }

        public virtual User Author { get; protected set; }
        public virtual Picture Picture { get; protected set; }
        public virtual string Text { get; protected set; }
        public virtual DateTimeOffset PostDate { get; protected set; }

        public virtual IReadOnlyList<Comment> Comments => _comments.ToList();

        public virtual IReadOnlyList<Like> Likes => _likes.ToList();

        public virtual void PutLikeFor(User user)
        {
            if (!LikesTo(user))
            {
                _likes.Add(new Like(this, user));
            }
        }

        public virtual void RemoveLikeBy(User user)
        {
            if (LikesTo(user))
            {
                var likeToRemove = Likes.FirstOrDefault(like => like.User == user);
                _likes.Remove(likeToRemove);
            }           
        }

        public virtual bool LikesTo(User user)
        {
            return Likes.Any(l => l.User == user);
        }
    }
}
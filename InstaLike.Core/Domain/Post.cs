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
        {
            Author = author;
            Picture = picture;
            Text = comment;

            Date = DateTimeOffset.Now;
        }

        public virtual User Author { get; protected set; }
        public virtual Picture Picture { get; protected set; }
        public virtual string Text { get; protected set; }
        public virtual DateTimeOffset Date { get; protected set; }

        public virtual IReadOnlyList<Comment> Comments => _comments.ToList();

        public virtual IReadOnlyList<Like> Likes => _likes.ToList();

        public void Like(User user)
        {
            if (!Likes.Any(like => like.User == user))
            {
                _likes.Add(new Like(this, user));
            }
        }

        public void RemoveLike(User user)
        {
            var likeToRemove = Likes.FirstOrDefault(like => like.User == user);
            if (likeToRemove != null)
            {
                _likes.Remove(likeToRemove);
            }
        }
    }
}
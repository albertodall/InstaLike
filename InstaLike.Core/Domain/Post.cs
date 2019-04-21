﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

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

        public Post(User author, Picture picture, PostText text)
            : this()
        {
            Author = author;
            Picture = picture;
            _text = text;

            PostDate = DateTimeOffset.Now;
        }

        public virtual User Author { get; protected set; }
        public virtual Picture Picture { get; protected set; }

        private readonly string _text;
        public virtual PostText Text => (PostText)_text;

        public virtual DateTimeOffset PostDate { get; protected set; }

        public virtual IReadOnlyList<Comment> Comments => _comments.ToList();

        public virtual IReadOnlyList<Like> Likes => _likes.ToList();

        public virtual bool LikesTo(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return _likes.Any(u => u.User == user);
        }

        public virtual Result Like(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user == Author)
            {
                return Result.Fail("You cannot put a 'Like' on your own posts.");
            }

            if (!LikesTo(user))
            {
                _likes.Add(new Like(this, user));
                return Result.Ok();
            }

            return Result.Fail("You already 'Liked' this post.");
        }

        public virtual Result Dislike(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (LikesTo(user))
            {
                var likeToRemove = _likes.Single(l => l.User == user);
                _likes.Remove(likeToRemove);
                return Result.Ok();
            }

            return Result.Fail("You did not put a 'Like' on this post.");
        }

        public virtual void AddComment(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }
            _comments.Add(comment);
        }
    }
}
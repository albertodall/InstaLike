﻿using System;

namespace InstaLike.Core.Domain
{
    public class Comment : EntityBase<int>
    {
        protected Comment() { }

        public Comment(Post post, User author, CommentText text) : this()
        {
            Post = post;
            Author = author;
            _text = text ?? throw new ArgumentNullException(nameof(text));
            CommentDate = DateTimeOffset.Now;
        }

        public virtual Post Post { get; }
        public virtual User Author { get; }

        private readonly string _text;
        public virtual CommentText Text => (CommentText)_text;

        public virtual DateTimeOffset CommentDate { get; }
    }
}
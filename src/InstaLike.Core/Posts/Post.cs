using System;
using System.Collections.Generic;
using System.Linq;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts
{
    public class Post : AggregateRoot<PostId>
    {
#pragma warning disable CS8618
        protected Post() { }
#pragma warning restore CS8618

        public Post(MemberId authorID, PictureId picture, PostText text)
            : base(PostId.New())
        {
            AuthorID = authorID;
            Picture = picture;
            _text = text;

            PostDate = DateTimeOffset.UtcNow;
        }

        public virtual MemberId AuthorID { get; protected set; }

        public virtual PictureId Picture { get; protected set; }

        private string _text;
        public virtual PostText Text => (PostText)_text;

        private readonly IList<Like> _likes = [];
        public virtual IReadOnlyCollection<Like> Likes => _likes.AsReadOnly();

        private readonly IList<Comment> _comments = [];
        public virtual IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

        public virtual DateTimeOffset PostDate { get; protected set; }

        public virtual void UpdatePicture(PictureId picture)
        {
            Picture = picture;
        }

        public virtual void UpdateText(PostText text)
        {
            _text = text.Value;
        }

        public virtual Result LikeBy(MemberId memberID)
        {
            if (!CanBeLikedBy(memberID))
            {
                return Result.Failure(PostErrors.OwnPostLiked);
            }

            if (IsLikedBy(memberID))
            {
                return Result.Failure(PostErrors.AlreadyLiked);
            }

            _likes.Add(new Like(this, memberID));
            RaiseDomainEvent(new PostLikedEvent(ID, memberID, AuthorID));
            return Result.Ok();
        }

        public virtual Result DislikeBy(MemberId memberID)
        {
            var like = _likes.SingleOrDefault(l => l.MemberID == memberID);
            if (like is null)
            {
                return Result.Failure(PostErrors.NotLiked);
            }

            _likes.Remove(like);
            return Result.Ok();
        }

        public virtual bool CanBeLikedBy(MemberId memberID)
        {
            return AuthorID != memberID;
        }

        public virtual bool IsLikedBy(MemberId memberID)
        {
            return _likes.Any(l => l.MemberID == memberID);
        }

        public virtual Comment CommentBy(MemberId authorID, CommentText text)
        {
            var comment = new Comment(this, authorID, text);
            _comments.Add(comment);
            RaiseDomainEvent(new CommentPublishedEvent(ID, authorID, AuthorID));
            return comment;
        }

        public virtual bool CanBeEditedBy(MemberId memberID)
        {
            return AuthorID == memberID;
        }

        public virtual Result DeleteBy(MemberId memberID)
        {
            if (!CanBeEditedBy(memberID))
            {
                return Result.Failure(PostErrors.DeleteNotAllowed);
            }

            RaiseDomainEvent(new PostDeletedEvent(ID, AuthorID));
            return Result.Ok();
        }
    }
}

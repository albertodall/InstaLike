using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

public class Post : AggregateRoot<int>
{
#pragma warning disable CS8618
    protected Post() { }
#pragma warning restore CS8618

    public Post(int authorID, PictureId picture, PostText text)
        : this()
    {
        AuthorID = authorID;
        Picture = picture;
        _text = text;

        PostDate = DateTimeOffset.Now;
    }

    public virtual int AuthorID { get; protected set; }

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

    public virtual Result LikeBy(int userID)
    {
        if (!CanBeLikedBy(userID))
        {
            return Result.Failure("You cannot put a 'Like' on your own posts.");
        }

        if (IsLikedBy(userID))
        {
            return Result.Failure($"User {userID} already 'Liked' post {ID}.");
        }

        _likes.Add(new Like(this, userID));
        RaiseDomainEvent(new PostLikedEvent(ID, userID, AuthorID));
        return Result.Success();
    }

    public virtual Result DislikeBy(int userID)
    {
        var like = _likes.SingleOrDefault(l => l.UserID == userID);
        if (like is null)
        {
            return Result.Failure($"User {userID} did not put any 'Like' on post {ID}.");
        }

        _likes.Remove(like);
        return Result.Success();
    }

    public virtual bool CanBeLikedBy(int userID)
    {
        return AuthorID != userID;
    }

    public virtual bool IsLikedBy(int userID)
    {
        return _likes.Any(l => l.UserID == userID);
    }

    public virtual Comment CommentBy(int authorID, CommentText text)
    {
        var comment = new Comment(this, authorID, text);
        _comments.Add(comment);
        RaiseDomainEvent(new CommentPublishedEvent(ID, authorID, AuthorID));
        return comment;
    }

    public virtual bool CanBeEditedBy(int userID)
    {
        return AuthorID == userID;
    }
}

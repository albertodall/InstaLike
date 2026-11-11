using System;
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

    public virtual DateTimeOffset PostDate { get; protected set; }

    public virtual void UpdatePicture(PictureId picture)
    {
        Picture = picture;
    }

    public virtual void UpdateText(PostText text)
    {
        _text = text.Value;
    }

    /// <remarks>
    /// Duplicate likes are rejected by the caller: they depend on the other likes, not on the post.
    /// </remarks>
    public virtual Result<Like> LikeBy(int userID)
    {
        return AuthorID == userID
            ? Result.Failure<Like>("You cannot put a 'Like' on your own posts.")
            : Result.Success(Like.Create(ID, userID, AuthorID));
    }

    public virtual Comment CommentBy(int authorID, CommentText text)
    {
        return Comment.Create(ID, authorID, AuthorID, text);
    }

    public virtual bool CanBeEditedBy(int userID)
    {
        return AuthorID == userID;
    }
}

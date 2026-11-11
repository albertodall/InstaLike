using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

public class Post : EntityBase<int>
{
#pragma warning disable CS8618
    protected Post() { }
#pragma warning restore CS8618

    public Post(User author, PictureId picture, PostText text)
        : this()
    {
        Author = author;
        Picture = picture;
        _text = text;

        PostDate = DateTimeOffset.Now;
    }

    public virtual User Author { get; protected set; }

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

    /// <remarks>Duplicate likes are rejected by the caller: they depend on the other likes, not on the post.</remarks>
    public virtual Result<Like> LikeBy(int userID)
    {
        return Author.ID == userID
            ? Result.Failure<Like>("You cannot put a 'Like' on your own posts.")
            : Result.Success(new Like(ID, userID));
    }

    public virtual Comment CommentBy(int authorID, CommentText text)
    {
        return new Comment(ID, authorID, text);
    }

    public virtual bool CanBeEditedBy(User user)
    {
        return Author == user;
    }
}

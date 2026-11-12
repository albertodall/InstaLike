using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

/// <summary>
/// Represents a comment on a post.
/// </summary>
public class Comment : EntityBase<int>
{
#pragma warning disable CS8618
    protected Comment() { }
#pragma warning restore CS8618

    internal Comment(Post post, int authorID, CommentText text)
        : this()
    {
        Post = post;
        AuthorID = authorID;
        _text = text ?? throw new ArgumentNullException(nameof(text));
        CommentDate = DateTimeOffset.Now;
    }

    public virtual Post Post { get; protected set; }

    public virtual int AuthorID { get; protected set; }

    private readonly string _text;
    public virtual CommentText Text => (CommentText)_text;

    public virtual DateTimeOffset CommentDate { get; protected set; }
}

using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

public class Comment : AggregateRoot<int>
{
#pragma warning disable CS8618
    protected Comment() { }
#pragma warning restore CS8618

    private Comment(int postID, int authorID, CommentText text)
        : this()
    {
        PostID = postID;
        AuthorID = authorID;
        _text = text ?? throw new ArgumentNullException(nameof(text));
        CommentDate = DateTimeOffset.Now;
    }

    public virtual int PostID { get; protected set; }

    public virtual int AuthorID { get; protected set; }

    private readonly string _text;
    public virtual CommentText Text => (CommentText)_text;

    public virtual DateTimeOffset CommentDate { get; protected set; }

    internal static Comment Create(int postID, int authorID, int postAuthorID, CommentText text)
    {
        var comment = new Comment(postID, authorID, text);
        comment.RaiseDomainEvent(new CommentPublishedEvent(postID, authorID, postAuthorID));
        return comment;
    }
}

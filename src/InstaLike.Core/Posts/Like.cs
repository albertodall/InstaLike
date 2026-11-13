using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

/// <summary>
/// Represents a user's like on a post.
/// </summary>
public class Like : EntityBase<Guid>
{
#pragma warning disable CS8618
    protected Like() { }
#pragma warning restore CS8618

    internal Like(Post post, Guid userID)
        : this()
    {
        Post = post;
        UserID = userID;
        LikeDate = DateTimeOffset.Now;
    }

    public virtual Post Post { get; protected set; }

    public virtual Guid UserID { get; protected set; }

    public virtual DateTimeOffset LikeDate { get; protected set; }
}

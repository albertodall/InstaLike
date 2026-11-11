using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

public class Like : EntityBase<int>
{
    protected Like() { }

    internal Like(int postID, int userID)
        : this()
    {
        PostID = postID;
        UserID = userID;
        LikeDate = DateTimeOffset.Now;
    }

    public virtual int PostID { get; protected set; }

    public virtual int UserID { get; protected set; }

    public virtual DateTimeOffset LikeDate { get; protected set; }
}

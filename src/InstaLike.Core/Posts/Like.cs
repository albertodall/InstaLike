using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Posts;

public class Like : AggregateRoot<int>
{
    protected Like() { }

    private Like(int postID, int userID)
        : this()
    {
        PostID = postID;
        UserID = userID;
        LikeDate = DateTimeOffset.Now;
    }

    public virtual int PostID { get; protected set; }

    public virtual int UserID { get; protected set; }

    public virtual DateTimeOffset LikeDate { get; protected set; }

    internal static Like Create(int postID, int userID, int postAuthorID)
    {
        var like = new Like(postID, userID);
        like.RaiseDomainEvent(new PostLikedEvent(postID, userID, postAuthorID));
        return like;
    }
}

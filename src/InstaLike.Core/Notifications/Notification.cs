using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Notifications;

/// <summary>
/// Represents a notification sent to a recipient when another user follows them,
/// likes their post, or comments on their post.
/// </summary>
public class Notification : AggregateRoot<int>
{
    protected Notification() { }

    private Notification(NotificationKind kind, int senderID, int recipientID, int? postID)
        : this()
    {
        Kind = kind;
        SenderID = senderID;
        RecipientID = recipientID;
        PostID = postID;
        HasBeenReadByRecipient = false;
        NotificationDate = DateTimeOffset.Now;
    }

    public virtual NotificationKind Kind { get; protected set; }

    public virtual int SenderID { get; protected set; }

    public virtual int RecipientID { get; protected set; }

    public virtual int? PostID { get; protected set; }

    public virtual bool HasBeenReadByRecipient { get; protected set; }

    public virtual DateTimeOffset NotificationDate { get; protected set; }

    public static Notification Followed(int followerID, int followedID) =>
        new(NotificationKind.Followed, followerID, followedID, null);

    public static Notification PostLiked(int likerID, int postAuthorID, int postID) =>
        new(NotificationKind.PostLiked, likerID, postAuthorID, postID);

    public static Notification PostCommented(int commentAuthorID, int postAuthorID, int postID) =>
        new(NotificationKind.PostCommented, commentAuthorID, postAuthorID, postID);

    public virtual void MarkAsRead()
    {
        if (!HasBeenReadByRecipient)
        {
            HasBeenReadByRecipient = true;
        }
    }

    public virtual void MarkAsUnread()
    {
        if (HasBeenReadByRecipient)
        {
            HasBeenReadByRecipient = false;
        }
    }
}

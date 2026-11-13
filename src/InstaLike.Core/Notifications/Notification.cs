using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Notifications
{
    /// <summary>
    /// Represents a notification sent to a recipient when another user follows them,
    /// likes their post, or comments on their post.
    /// </summary>
    public class Notification : AggregateRoot<NotificationId>
    {
        protected Notification() { }

        private Notification(NotificationKind kind, Guid senderID, Guid recipientID, Guid? postID)
            : base(NotificationId.New())
        {
            Kind = kind;
            SenderID = senderID;
            RecipientID = recipientID;
            PostID = postID;
            HasBeenReadByRecipient = false;
            NotificationDate = DateTimeOffset.Now;
        }

        public virtual NotificationKind Kind { get; protected set; }

        public virtual Guid SenderID { get; protected set; }

        public virtual Guid RecipientID { get; protected set; }

        public virtual Guid? PostID { get; protected set; }

        public virtual bool HasBeenReadByRecipient { get; protected set; }

        public virtual DateTimeOffset NotificationDate { get; protected set; }

        public static Notification Followed(Guid followerID, Guid followedID) =>
            new(NotificationKind.Followed, followerID, followedID, null);

        public static Notification PostLiked(Guid likerID, Guid postAuthorID, Guid postID) =>
            new(NotificationKind.PostLiked, likerID, postAuthorID, postID);

        public static Notification PostCommented(Guid commentAuthorID, Guid postAuthorID, Guid postID) =>
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
}

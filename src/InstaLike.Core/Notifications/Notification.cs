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
#pragma warning disable CS8618
        protected Notification() { }
#pragma warning restore CS8618

        private Notification(NotificationKind kind, MemberId senderID, MemberId recipientID, PostId? postID)
            : base(NotificationId.New())
        {
            Kind = kind;
            SenderID = senderID;
            RecipientID = recipientID;
            PostID = postID;
            HasBeenReadByRecipient = false;
            NotificationDate = DateTimeOffset.UtcNow;
        }

        public virtual NotificationKind Kind { get; protected set; }

        public virtual MemberId SenderID { get; protected set; }

        public virtual MemberId RecipientID { get; protected set; }

        public virtual PostId? PostID { get; protected set; }

        public virtual bool HasBeenReadByRecipient { get; protected set; }

        public virtual DateTimeOffset NotificationDate { get; protected set; }

        public static Notification Followed(MemberId followerID, MemberId followedID) =>
            new(NotificationKind.Followed, followerID, followedID, null);

        public static Notification PostLiked(MemberId likerID, MemberId postAuthorID, PostId postID) =>
            new(NotificationKind.PostLiked, likerID, postAuthorID, postID);

        public static Notification PostCommented(MemberId commentAuthorID, MemberId postAuthorID, PostId postID) =>
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

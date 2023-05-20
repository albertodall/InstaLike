using System;

namespace InstaLike.Core.Domain
{
    public class Notification : EntityBase<int>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected Notification() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Notification(User sender, User recipient, string message) : this()
        {
            Sender = sender;
            Recipient = recipient;
            Message = message;

            HasBeenReadByRecipient = false;
            NotificationDate = DateTimeOffset.Now;
        }

        public virtual User Sender { get; }
        public virtual User Recipient { get; }
        public virtual string Message { get; }
        public virtual bool HasBeenReadByRecipient { get; protected set; }
        public virtual DateTimeOffset NotificationDate { get; }

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

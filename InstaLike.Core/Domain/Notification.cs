using System;

namespace InstaLike.Core.Domain
{
    public class Notification : EntityBase<int>
    {
        protected Notification()
        { }

        public Notification(User sender, User recipient, string message)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
            Message = message ?? throw new ArgumentNullException(nameof(message));

            IsRead = false;
            NotificationDate = DateTimeOffset.Now;
        }

        public virtual User Sender { get; protected set; }
        public virtual User Recipient { get; protected set; }
        public virtual string Message { get; protected set; }
        public virtual bool IsRead { get; protected set; }
        public virtual DateTimeOffset NotificationDate { get; protected set; }

        public virtual void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
            }
        }

        public virtual void MarkAsUnread()
        {
            if (IsRead)
            {
                IsRead = false;
            }
        }
    }
}

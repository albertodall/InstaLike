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
            DateTime = DateTimeOffset.Now;
        }

        public User Sender { get; protected set; }
        public User Recipient { get; protected set; }
        public string Message { get; protected set; }
        public bool IsRead { get; protected set; }
        public DateTimeOffset DateTime { get; protected set; }

        public virtual void SetNotificationRead()
        {
            IsRead = true;
        }

        public virtual void SetNotificationUnread()
        {
            IsRead = false;
        }
    }
}

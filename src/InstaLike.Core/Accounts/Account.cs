using System;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts
{
    /// <summary>
    /// Represents the credentials a member logs in with.
    /// </summary>
    public class Account : AggregateRoot<AccountId>
    {
        private const int MaximumFailedLogInAttempts = 5;
        private static readonly TimeSpan LockOutDuration = TimeSpan.FromMinutes(15);

#pragma warning disable CS8618
        protected Account() { }
#pragma warning restore CS8618

        private Account(Guid memberID, Username username, Password password, Email email)
            : base(AccountId.From(memberID))
        {
            _username = username;
            _password = password;
            _email = email;
            SecurityStamp = Guid.NewGuid();
        }

        private readonly string _username;
        public virtual Username Username => (Username)_username;

        private string _password;
        public virtual Password Password => (Password)_password;

        private string _email;
        public virtual Email Email => (Email)_email;

        public virtual Guid SecurityStamp { get; protected set; }

        public virtual int FailedLogInAttempts { get; protected set; }

        public virtual DateTimeOffset? LockedOutUntil { get; protected set; }

        public static Account Create(Guid memberID, Username username, Password password, Email email) =>
            new(memberID, username, password, email);

        public virtual Result LogIn(string password)
        {
            if (IsLockedOut())
            {
                return Result.Failure(AccountErrors.LockedOut);
            }

            if (!Password.HashMatches(password))
            {
                CountFailedLogIn();
                return Result.Failure(AccountErrors.InvalidCredentials);
            }

            FailedLogInAttempts = 0;
            RaiseDomainEvent(new AccountLoggedInEvent(ID));
            return Result.Ok();
        }

        public virtual void LogOut()
        {
            RaiseDomainEvent(new AccountLoggedOutEvent(ID));
        }

        public virtual Result ChangePassword(string currentPassword, Password newPassword)
        {
            if (!Password.HashMatches(currentPassword))
            {
                return Result.Failure(AccountErrors.WrongCurrentPassword);
            }

            _password = newPassword;
            SecurityStamp = Guid.NewGuid();
            RaiseDomainEvent(new AccountPasswordChangedEvent(ID));
            return Result.Ok();
        }

        public virtual void ChangeEmailAddress(Email email)
        {
            _email = email;
        }

        private bool IsLockedOut()
        {
            return LockedOutUntil > DateTimeOffset.UtcNow;
        }

        private void CountFailedLogIn()
        {
            FailedLogInAttempts++;
            if (FailedLogInAttempts < MaximumFailedLogInAttempts)
            {
                return;
            }

            FailedLogInAttempts = 0;
            LockedOutUntil = DateTimeOffset.UtcNow + LockOutDuration;
            RaiseDomainEvent(new AccountLockedOutEvent(ID));
        }
    }
}

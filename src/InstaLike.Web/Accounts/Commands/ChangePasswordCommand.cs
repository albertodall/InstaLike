using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands
{
    public sealed class ChangePasswordCommand : ICommand<Result>
    {
        public AccountId AccountID { get; }
        public string CurrentPassword { get; }
        public string NewPassword { get; }

        public ChangePasswordCommand(AccountId accountID, string currentPassword, string newPassword)
        {
            AccountID = accountID;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }
    }
}

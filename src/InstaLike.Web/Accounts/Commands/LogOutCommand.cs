using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands
{
    public sealed class LogOutCommand : ICommand<Result>
    {
        public AccountId AccountID { get; }

        public LogOutCommand(AccountId accountID)
        {
            AccountID = accountID;
        }
    }
}

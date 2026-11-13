using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

public sealed class ChangeEmailCommand : ICommand<Result>
{
    public AccountId AccountID { get; }
    public string Email { get; }

    public ChangeEmailCommand(AccountId accountID, string email)
    {
        AccountID = accountID;
        Email = email;
    }
}

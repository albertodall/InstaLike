using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

public sealed class LogOutCommand : ICommand<Result>
{
    public UserId UserID { get; }

    public LogOutCommand(UserId userID)
    {
        UserID = userID;
    }
}

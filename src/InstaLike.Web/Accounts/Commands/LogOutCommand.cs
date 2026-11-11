using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

public sealed class LogOutCommand : ICommand<Result>
{
    public int UserID { get; }

    public LogOutCommand(int userID)
    {
        UserID = userID;
    }
}

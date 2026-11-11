using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

public sealed class LogOutCommand : ICommand<Result>
{
    public int UserID { get; }

    public LogOutCommand(int userID)
    {
        UserID = userID;
    }
}

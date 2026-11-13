using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

public sealed class LogOutCommand : ICommand<Result>
{
    public MemberId MemberID { get; }

    public LogOutCommand(MemberId memberID)
    {
        MemberID = memberID;
    }
}

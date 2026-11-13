using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

public sealed class LogInCommand : ICommand<Result<Member>>
{
    public string Nickname { get; }
    public string Password { get; }

    public LogInCommand(string nickname, string password)
    {
        Nickname = nickname;
        Password = password;
    }
}

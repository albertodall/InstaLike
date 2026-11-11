using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

public sealed class LogInCommand : ICommand<Result<User>>
{
    public string Nickname { get; }
    public string Password { get; }

    public LogInCommand(string nickname, string password)
    {
        Nickname = nickname;
        Password = password;
    }
}

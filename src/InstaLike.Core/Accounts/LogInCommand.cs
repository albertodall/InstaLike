using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

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

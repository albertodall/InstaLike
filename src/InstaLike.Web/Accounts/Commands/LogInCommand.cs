using System;
using CSharpFunctionalExtensions;
using InstaLike.Web.Data;

namespace InstaLike.Web.Accounts.Commands;

/// <summary>
/// Logs an account in; the result is the identifier of the member the account belongs to.
/// </summary>
public sealed class LogInCommand : ICommand<Result<Guid>>
{
    public string Username { get; }
    public string Password { get; }

    public LogInCommand(string username, string password)
    {
        Username = username;
        Password = password;
    }
}

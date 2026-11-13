using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Represents the credentials a member logs in with.
/// </summary>
public class Account : AggregateRoot<AccountId>
{
#pragma warning disable CS8618
    protected Account() { }
#pragma warning restore CS8618

    private Account(Guid memberID, Username username, Password password, Email email)
        : base(AccountId.From(memberID))
    {
        _username = username;
        _password = password;
        _email = email;
    }

    private string _username;
    public virtual Username Username => (Username)_username;

    private string _password;
    public virtual Password Password => (Password)_password;

    private string _email;
    public virtual Email Email => (Email)_email;

    public static Account Create(Guid memberID, Username username, Password password, Email email) =>
        new(memberID, username, password, email);

    public virtual Result LogIn(string password)
    {
        if (!Password.HashMatches(password))
        {
            return Result.Failure("Username or password are not valid.");
        }

        RaiseDomainEvent(new AccountLoggedInEvent(ID));
        return Result.Success();
    }

    public virtual void LogOut()
    {
        RaiseDomainEvent(new AccountLoggedOutEvent(ID));
    }

    public virtual Result ChangePassword(string currentPassword, Password newPassword)
    {
        if (!Password.HashMatches(currentPassword))
        {
            return Result.Failure("The current password is not valid.");
        }

        _password = newPassword;
        return Result.Success();
    }

    public virtual void ChangeEmailAddress(Email email)
    {
        _email = email;
    }
}

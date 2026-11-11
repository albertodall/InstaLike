using System;
using CSharpFunctionalExtensions;
using InstaLike.Core.SharedKernel;

namespace InstaLike.Core.Accounts;

/// <summary>
/// Represents a user account with profile information and authentication capabilities.
/// </summary>
public class User : AggregateRoot<int>
{
#pragma warning disable CS8618
    protected User() { }
#pragma warning restore CS8618

    public User(Nickname nickname, FullName fullName, Password password, Email email, string biography)
        : this()
    {
        _nickname = nickname;
        _email = email;
        _password = password;
        _fullName = fullName;
        Biography = biography;
        RegistrationDate = DateTimeOffset.Now;
    }

    private string _nickname;
    public virtual Nickname Nickname
    {
        get => (Nickname)_nickname;
        protected set => _nickname = value;
    }

    private string _fullName;
    public virtual FullName FullName
    {
        get => (FullName)_fullName;
        protected set => _fullName = value;
    }

    private string _password;
    public virtual Password Password
    {
        get => (Password)_password;
        set => _password = value;
    }

    private string _email;
    public virtual Email Email
    {
        get => (Email)_email;
        set => _email = value;
    }

    public virtual PictureId? ProfilePicture { get; protected set; }
    public virtual string Biography { get; protected set; }
    public virtual DateTimeOffset RegistrationDate { get; protected set; }

    public virtual Result LogIn(string password)
    {
        if (!Password.HashMatches(password))
        {
            return Result.Failure("Username or password are not valid.");
        }

        RaiseDomainEvent(new UserLoggedInEvent(ID));
        return Result.Success();
    }

    public virtual void LogOut()
    {
        RaiseDomainEvent(new UserLoggedOutEvent(ID));
    }

    public virtual void ChangePassword(Password password)
    {
        Password = password;
    }

    public virtual void SetProfilePicture(PictureId picture)
    {
        ProfilePicture = picture;
    }

    public virtual void RemoveProfilePicture()
    {
        ProfilePicture = null;
    }

    public virtual void ChangeNickname(Nickname nickname)
    {
        Nickname = nickname;
    }

    public virtual void ChangeFullName(FullName fullName)
    {
        FullName = fullName;
    }

    public virtual void ChangeEmailAddress(Email email)
    {
        Email = email;
    }

    public virtual void UpdateBiography(string newBiography)
    {
        Biography = newBiography;
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

internal sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
{
    private const string NicknameTaken = "This nickname is already taken.";
    private const string EmailInUse = "This email is already used by another account.";

    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public RegisterUserCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session;
        _logger = logger.ForContext<RegisterUserCommand>();
        _pictureStore = pictureStore;
    }

    public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var nicknameValidationResult = Nickname.Create(request.Nickname);
        var eMailValidationResult = Email.Create(request.Email);
        var passwordValidationResult = Password.Create(request.Password);
        var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

        var validationResult = Result.Combine(Environment.NewLine, nicknameValidationResult, eMailValidationResult, passwordValidationResult, fullNameValidationResult);
        if (validationResult.IsFailure)
        {
            _logger.Warning("Error during registration of user {Nickname}: Error message: {ErrorMessage}",
                request.Nickname,
                validationResult.Error);
            return Result.Failure<int>(validationResult.Error);
        }

        var availabilityResult = await CheckAvailabilityAsync(nicknameValidationResult.Value, eMailValidationResult.Value, cancellationToken);
        if (availabilityResult.IsFailure)
        {
            _logger.Warning("Error during registration of user {Nickname}: Error message: {ErrorMessage}",
                request.Nickname,
                availabilityResult.Error);
            return Result.Failure<int>(availabilityResult.Error);
        }

        var userToRegister = new User(
            nicknameValidationResult.Value,
            fullNameValidationResult.Value,
            passwordValidationResult.Value,
            eMailValidationResult.Value,
            request.Biography);

        if (request.ProfilePicture is { Length: > 0 })
        {
            var profilePicture = await _pictureStore.SaveAsync(PictureContainers.Profiles, request.ProfilePicture, cancellationToken);
            userToRegister.SetProfilePicture(profilePicture);
        }

        try
        {
            await _session.SaveAsync(userToRegister, cancellationToken);
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Error during registration of user {Nickname}. Error message: {ErrorMessage}",
                request.Nickname,
                ex.Message);

            return Result.Failure<int>(ex.Message);
        }

        _logger.Information("User [{Nickname}({UserID})] has just registered.",
            request.Nickname,
            userToRegister.ID);

        return Result.Success(userToRegister.ID);
    }

    private async Task<Result> CheckAvailabilityAsync(Nickname nickname, Email email, CancellationToken cancellationToken)
    {
        var usersWithNickname = _session.QueryOver<User>()
            .Where(Restrictions.Eq("Nickname", nickname.Value))
            .ToRowCountQuery()
            .FutureValue<int>();
        var usersWithEmail = _session.QueryOver<User>()
            .Where(Restrictions.Eq("Email", email.Value))
            .ToRowCountQuery()
            .FutureValue<int>();

        return Result.Combine(
            Environment.NewLine,
            Result.FailureIf(await usersWithNickname.GetValueAsync(cancellationToken) > 0, NicknameTaken),
            Result.FailureIf(await usersWithEmail.GetValueAsync(cancellationToken) > 0, EmailInUse));
    }
}

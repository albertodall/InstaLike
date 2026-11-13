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

public sealed class EditUserDetailsCommandHandler : IRequestHandler<EditUserDetailsCommand, Result>
{
    private const string NicknameTaken = "This nickname is already taken.";
    private const string EmailInUse = "This email is already used by another account.";

    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public EditUserDetailsCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session;
        _logger = logger.ForContext<EditUserDetailsCommand>();
        _pictureStore = pictureStore;
    }

    public async Task<Result> Handle(EditUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var nicknameValidationResult = Nickname.Create(request.Nickname);
        var eMailValidationResult = Email.Create(request.Email);
        var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

        var validationResult = Result.Combine(Environment.NewLine, nicknameValidationResult, eMailValidationResult, fullNameValidationResult);
        if (validationResult.IsFailure)
        {
            _logger.Warning("Tried to update user profile for user [{Nickname}({UserID})] but some data were not valid: {WarningMessage}",
                request.Nickname,
                request.UserID,
                validationResult.Error);
            return Result.Failure(validationResult.Error);
        }

        var availabilityResult = await CheckAvailabilityAsync(request.UserID, nicknameValidationResult.Value, eMailValidationResult.Value, cancellationToken);
        if (availabilityResult.IsFailure)
        {
            _logger.Warning("Tried to update user profile for user [{Nickname}({UserID})] but some data were already in use: {WarningMessage}",
                request.Nickname,
                request.UserID,
                availabilityResult.Error);
            return availabilityResult;
        }

        var userToUpdate = await _session.GetAsync<User>(request.UserID, cancellationToken);
        userToUpdate.ChangeNickname(nicknameValidationResult.Value);
        userToUpdate.ChangeEmailAddress(eMailValidationResult.Value);
        userToUpdate.ChangeFullName(fullNameValidationResult.Value);
        userToUpdate.UpdateBiography(request.Bio);
        if (request.ProfilePicture is { Length: > 0 })
        {
            var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Profiles, request.ProfilePicture, cancellationToken);
            userToUpdate.SetProfilePicture(uploadedPicture);
        }

        try
        {
            await _session.FlushAsync(cancellationToken);
        }
        catch (ADOException ex)
        {
            _logger.Error("Error updating user profile for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                request.Nickname,
                request.UserID,
                ex.Message);

            return Result.Failure(ex.Message);
        }

        _logger.Information("Successfully updated user profile for user [{Nickname}({UserID})]",
            request.Nickname,
            request.UserID);

        return Result.Success();
    }

    private async Task<Result> CheckAvailabilityAsync(UserId userID, Nickname nickname, Email email, CancellationToken cancellationToken)
    {
        var otherUsersWithNickname = _session.QueryOver<User>()
            .Where(Restrictions.Eq("Nickname", nickname.Value))
            .And(u => u.ID != userID)
            .ToRowCountQuery()
            .FutureValue<int>();
        var otherUsersWithEmail = _session.QueryOver<User>()
            .Where(Restrictions.Eq("Email", email.Value))
            .And(u => u.ID != userID)
            .ToRowCountQuery()
            .FutureValue<int>();

        return Result.Combine(
            Environment.NewLine,
            Result.FailureIf(await otherUsersWithNickname.GetValueAsync(cancellationToken) > 0, NicknameTaken),
            Result.FailureIf(await otherUsersWithEmail.GetValueAsync(cancellationToken) > 0, EmailInUse));
    }
}

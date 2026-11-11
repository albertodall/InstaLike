using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Data;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

public sealed class EditUserDetailsCommandHandler : IRequestHandler<EditUserDetailsCommand, Result>
{
    private const string EmailInUse = "This email is already used by another account.";

    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;
    private readonly IUnitOfWork _unitOfWork;

    public EditUserDetailsCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore, IUnitOfWork unitOfWork)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<EditUserDetailsCommand>() ?? throw new ArgumentNullException(nameof(logger));
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result> Handle(EditUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var nicknameValidationResult = Nickname.Create(request.Nickname);
        var eMailValidationResult = Email.Create(request.Email);
        var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

        var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, fullNameValidationResult);
        if (validationResult.IsFailure)
        {
            _logger.Warning("Tried to update user profile for user [{Nickname}({UserID})] but some data were not valid: {WarningMessage}",
                request.Nickname,
                request.UserID,
                validationResult.Error);
            return Result.Failure(validationResult.Error);
        }

        var availabilityResult = await CheckAvailabilityAsync(request.UserID, eMailValidationResult.Value, cancellationToken);
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
            var replacedPicture = userToUpdate.ProfilePicture;
            var uploadedPicture = await _pictureStore.SaveAsync(PictureContainers.Profiles, request.ProfilePicture, cancellationToken);
            if (replacedPicture is not null)
            {
                _unitOfWork.OnCommitted(() => _pictureStore.TryDeleteAsync(PictureContainers.Profiles, replacedPicture));
            }

            _unitOfWork.OnRolledBack(() => _pictureStore.TryDeleteAsync(PictureContainers.Profiles, uploadedPicture));
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

    private async Task<Result> CheckAvailabilityAsync(int userID, Email email, CancellationToken cancellationToken)
    {
        var otherUsersWithEmail = await _session.QueryOver<User>()
            .Where(Restrictions.Eq("Email", email.Value))
            .And(u => u.ID != userID)
            .RowCountAsync(cancellationToken);

        return Result.FailureIf(otherUsersWithEmail > 0, EmailInUse);
    }
}

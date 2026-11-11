using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.SharedKernel;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

public sealed class EditUserDetailsCommandHandler : IRequestHandler<EditUserDetailsCommand, Result>
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public EditUserDetailsCommandHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<EditUserDetailsCommand>() ?? throw new ArgumentNullException(nameof(logger));
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

        using (var tx = _session.BeginTransaction())
        {
            try
            {
                var userToUpdate = await _session.GetAsync<User>(request.UserID, cancellationToken);
                userToUpdate.ChangeNickname(nicknameValidationResult.Value);
                userToUpdate.ChangeEmailAddress(eMailValidationResult.Value);
                userToUpdate.ChangeFullName(fullNameValidationResult.Value);
                userToUpdate.UpdateBiography(request.Bio);
                if (request.ProfilePicture is { Length: > 0 })
                {
                    userToUpdate.SetProfilePicture(Picture.Create(request.ProfilePicture, Guid.NewGuid()).Value);
                }

                await tx.CommitAsync(cancellationToken);

                _logger.Information("Successfully updated user profile for user [{Nickname}({UserID})]",
                    request.Nickname,
                    request.UserID);

                return Result.Success(userToUpdate.ID);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);

                _logger.Error("Error updating user profile for user [{Nickname}({UserID})]. Error message: {ErrorMessage}",
                    request.Nickname,
                    request.UserID,
                    ex.Message);

                return Result.Failure(ex.Message);
            }
        }
    }
}

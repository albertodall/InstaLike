using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Media;
using MediatR;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
{
    private readonly ISession _session;
    private readonly ILogger _logger;
    private readonly IPictureStore _pictureStore;

    public RegisterUserCommandHandler(ISession session, ILogger logger, IPictureStore pictureStore)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<RegisterUserCommand>() ?? throw new ArgumentNullException(nameof(logger));
        _pictureStore = pictureStore ?? throw new ArgumentNullException(nameof(pictureStore));
    }

    public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var nicknameValidationResult = Nickname.Create(request.Nickname);
        var eMailValidationResult = Email.Create(request.Email);
        var passwordValidationResult = Password.Create(request.Password);
        var fullNameValidationResult = FullName.Create(request.Name, request.Surname);

        var validationResult = Result.Combine(nicknameValidationResult, eMailValidationResult, passwordValidationResult, fullNameValidationResult);
        if (validationResult.IsFailure)
        {
            _logger.Warning("Error during registration of user {Nickname}: Error message: {ErrorMessage}",
                request.Nickname,
                validationResult.Error);
            return Result.Failure<int>(validationResult.Error);
        }

        var userToRegister = new User(
            nicknameValidationResult.Value,
            fullNameValidationResult.Value,
            passwordValidationResult.Value,
            eMailValidationResult.Value,
            request.Biography);

        if (request.ProfilePicture is { Length: > 0 })
        {
            userToRegister.SetProfilePicture(await _pictureStore.SaveAsync(PictureContainers.Profiles, request.ProfilePicture, cancellationToken));
        }

        using (var tx = _session.BeginTransaction())
        {
            try
            {
                await _session.SaveAsync(userToRegister, cancellationToken);
                await tx.CommitAsync(cancellationToken);

                _logger.Information("User [{Nickname}({UserID})] has just registered.",
                    request.Nickname,
                    userToRegister.ID);

                return Result.Success(userToRegister.ID);
            }
            catch (ADOException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                await _pictureStore.TryDeleteAsync(PictureContainers.Profiles, userToRegister.ProfilePicture);

                _logger.Error("Error during registration of user {Nickname}. Error message: {ErrorMessage}",
                    request.Nickname,
                    ex.Message);

                return Result.Failure<int>(ex.Message);
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

internal sealed class LogInCommandHandler : IRequestHandler<LogInCommand, Result<User>>
{
    private const string InvalidCredentials = "Username or password are not valid.";

    private readonly ISession _session;
    private readonly ILogger _logger;

    public LogInCommandHandler(ISession session, ILogger logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger?.ForContext<LogInCommand>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<User>> Handle(LogInCommand request, CancellationToken cancellationToken)
    {
        var user = await _session.QueryOver<User>()
            .Where(Restrictions.Eq("Nickname", request.Nickname))
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            _logger.Debug("User {Nickname} does not exist.", request.Nickname);
            return Result.Failure<User>(InvalidCredentials);
        }

        var logIn = user.LogIn(request.Password);
        if (logIn.IsFailure)
        {
            _logger.Debug("User {Nickname} did not authenticate correctly.", request.Nickname);
            return Result.Failure<User>(logIn.Error);
        }

        return Result.Success(user);
    }
}

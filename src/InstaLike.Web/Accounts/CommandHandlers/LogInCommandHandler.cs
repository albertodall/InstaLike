using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

internal sealed class LogInCommandHandler : IRequestHandler<LogInCommand, Result<Guid>>
{
    private const string InvalidCredentials = "Username or password are not valid.";

    private readonly ISession _session;
    private readonly ILogger _logger;

    public LogInCommandHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<LogInCommand>();
    }

    public async Task<Result<Guid>> Handle(LogInCommand request, CancellationToken cancellationToken)
    {
        var account = await _session.QueryOver<Account>()
            .Where(Restrictions.Eq("Username", request.Username))
            .SingleOrDefaultAsync(cancellationToken);
        if (account is null)
        {
            _logger.Debug("Account {Username} does not exist.", request.Username);
            return Result.Failure<Guid>(InvalidCredentials);
        }

        var logIn = account.LogIn(request.Password);
        if (logIn.IsFailure)
        {
            _logger.Debug("Account {Username} did not authenticate correctly.", request.Username);
            return Result.Failure<Guid>(logIn.Error);
        }

        return Result.Success(account.ID.Value);
    }
}

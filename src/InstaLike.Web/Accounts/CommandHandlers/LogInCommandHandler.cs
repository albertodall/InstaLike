using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;
using InstaLike.Core.Membership;
using InstaLike.Web.Accounts.Commands;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Accounts.CommandHandlers;

internal sealed class LogInCommandHandler : IRequestHandler<LogInCommand, Result<Member>>
{
    private const string InvalidCredentials = "Username or password are not valid.";

    private readonly ISession _session;
    private readonly ILogger _logger;

    public LogInCommandHandler(ISession session, ILogger logger)
    {
        _session = session;
        _logger = logger.ForContext<LogInCommand>();
    }

    public async Task<Result<Member>> Handle(LogInCommand request, CancellationToken cancellationToken)
    {
        var member = await _session.QueryOver<Member>()
            .Where(Restrictions.Eq("Nickname", request.Nickname))
            .SingleOrDefaultAsync(cancellationToken);
        if (member is null)
        {
            _logger.Debug("Member {Nickname} does not exist.", request.Nickname);
            return Result.Failure<Member>(InvalidCredentials);
        }

        var logIn = member.LogIn(request.Password);
        if (logIn.IsFailure)
        {
            _logger.Debug("Member {Nickname} did not authenticate correctly.", request.Nickname);
            return Result.Failure<Member>(logIn.Error);
        }

        return Result.Success(member);
    }
}

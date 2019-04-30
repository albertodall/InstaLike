using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using NHibernate;
using NHibernate.Criterion;
using Serilog;

namespace InstaLike.Web.Services
{
    internal sealed class DatabaseAuthenticationService : IUserAuthenticationService
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly ILogger _logger;

        public DatabaseAuthenticationService(ISessionFactory sessionFactory, ILogger logger)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            _logger = logger?.ForContext<DatabaseAuthenticationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<User>> AuthenticateUser(string userName, string password)
        {
            Maybe<User> userLoggingIn;

            _logger.Debug("Authenticating user {userName}", userName);

            using (var session = _sessionFactory.OpenStatelessSession())
            {
                var authQuery = session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", userName)); // Compares the private field

                userLoggingIn = await authQuery.SingleOrDefaultAsync();
            }

            return userLoggingIn
                .ToResult($"Username or password are not valid.")
                .Ensure(user => user.Password.HashMatches(password), "Username or password are not valid.")
                .OnSuccess(user => _logger.Debug("User {userName} authenticated correctly.", userName))
                .OnFailure(user => _logger.Debug("User {userName} did not authenticate correctly.", userName));
        }
    }
}

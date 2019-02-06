using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using NHibernate;

namespace InstaLike.Web.Services
{
    internal class DatabaseAuthenticationService : IUserAuthenticationService
    {
        private readonly ISessionFactory _sessionFactory;

        public DatabaseAuthenticationService(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        public async Task<Result> AuthenticateUser(string userName, string password)
        {
            var hashedPassword = (Password)password;
            bool authenticated = false;

            using (var session = _sessionFactory.OpenStatelessSession())
            {
                var authenticationQuery = session.QueryOver<User>()
                    .Where(u => u.Nickname == userName && u.Password == hashedPassword)
                    .Select(u => u.ID);

                authenticated = await authenticationQuery.SingleOrDefaultAsync() != null;
            }

            return authenticated ? Result.Ok(authenticated) : Result.Fail("Username or password not valid.");
        }
    }
}

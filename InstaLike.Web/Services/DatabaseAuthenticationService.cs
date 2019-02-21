using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;
using NHibernate;
using NHibernate.Criterion;

namespace InstaLike.Web.Services
{
    internal class DatabaseAuthenticationService : IUserAuthenticationService
    {
        private readonly ISessionFactory _sessionFactory;

        public DatabaseAuthenticationService(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        public async Task<Result<User>> AuthenticateUser(string userName, string password)
        {
            bool authenticated = false;
            User userLoggingIn = null;

            using (var session = _sessionFactory.OpenStatelessSession())
            {
                var authQuery = session.QueryOver<User>()
                    .Where(Restrictions.Eq("Nickname", userName)); // Compares the private field

                userLoggingIn = await authQuery.SingleOrDefaultAsync();
                if (userLoggingIn != null)
                {
                    authenticated = userLoggingIn.Password.HashMatches(password);
                }
            }

            return authenticated ? 
                Result.Ok(userLoggingIn) : 
                Result.Fail<User>("Username or password are not valid.");
        }      
    }
}

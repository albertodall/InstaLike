using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Accounts;

namespace InstaLike.Web.Accounts.Services;

public interface IUserAuthenticationService
{
    Task<Result<User>> AuthenticateUser(string userName, string password);
}

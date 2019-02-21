using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using InstaLike.Core.Domain;

namespace InstaLike.Web.Services
{
    public interface IUserAuthenticationService
    {
        Task<Result<User>> AuthenticateUser(string userName, string password);
    }
}

using System.Threading.Tasks;
using NectarineAPI.Models;

namespace NectarineAPI.Services.Auth
{
    public interface IExternalAuthService<T>
        where T : IExternalAuthUser
    {
        /// <summary>
        /// Get a user profile for an external auth platform through a token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<T?> GetUserFromTokenAsync(string token);
    }
}
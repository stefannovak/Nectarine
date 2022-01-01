using System.Threading.Tasks;
using nectarineAPI.Models;

namespace nectarineAPI.Services
{
    public interface ISocialService<T> where T : ISocialUser
    {
        /// <summary>
        /// Get a user profile for a social platform through a token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<T?> GetUserFromTokenAsync(string token);
    }
}
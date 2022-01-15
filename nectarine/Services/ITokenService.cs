using NectarineData.Models;

namespace NectarineAPI.Services
{
    public interface ITokenService
    {
        public string GenerateTokenAsync(ApplicationUser user);
    }
}
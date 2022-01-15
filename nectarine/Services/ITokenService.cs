using nectarineData.Models;

namespace nectarineAPI.Services
{
    public interface ITokenService
    {
        public string GenerateTokenAsync(ApplicationUser user);
    }
}
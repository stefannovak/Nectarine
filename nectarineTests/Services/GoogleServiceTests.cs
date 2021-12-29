using System.Threading.Tasks;
using nectarineAPI.Models;
using nectarineAPI.Services;
using Xunit;

namespace nectarineTests.Services
{
    public class GoogleServiceTests
    {
        private readonly GoogleService<GoogleUser> _subject = new();

        [Fact(DisplayName = "GetUserFromTokenAsync should return a GoogleUser")]
        public async Task Test_GetUserFromTokenAsync_ReturnsGoogleUser()
        {
            // Act
            var result = await _subject.GetUserFromTokenAsync("token");

            // Assert
            Assert.IsType<GoogleUser>(result);
        }
    }
}
using System.Threading.Tasks;
using Moq;
using nectarineAPI.Models;
using nectarineAPI.Services;
using Xunit;

namespace nectarineTests.Services
{
    public class GoogleServiceTests
    {
        private readonly GoogleService<GoogleUser> _subject = new();
        private readonly Mock<IExternalAuthService<GoogleUser>> _mockSubject = new();

        // TODO: - Figure out this test
        [Fact(DisplayName = "GetUserFromTokenAsync should return a GoogleUser")]
        public async Task Test_GetUserFromTokenAsync_ReturnsGoogleUser()
        {
            // Assert
            _mockSubject
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new GoogleUser());
            
            // Act
            var result = await _mockSubject.Object.GetUserFromTokenAsync("token");

            // Assert
            Assert.IsType<GoogleUser>(result);
        }
        
        [Fact(DisplayName = "GetUserFromTokenAsync should return null")]
        public async Task Test_GetUserFromTokenAsync_ReturnsNullWhen_InvalidGoogleUser()
        {
            // Act
            var result = await _subject.GetUserFromTokenAsync("token");

            // Assert
            Assert.Null(result);
        }
    }
}
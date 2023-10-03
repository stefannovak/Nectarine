using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Moq;
using NectarineAPI.Configurations;
using NectarineAPI.Services;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _subject;
        private readonly ApplicationUser _user = new ()
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
        };

        private readonly string expectedIssuer = "https://nectarine.com";
        private readonly string expectedAudience = "https://myaudience.com";

        public TokenServiceTests()
        {
            // Token Options setup
            var mockTokenOptions = new Mock<IOptions<TokenOptions>>();

            var options = new TokenOptions
            {
                Secret = "mySecretJwtToken",
                Audience = expectedAudience,
                Issuer = expectedIssuer,
            };

            mockTokenOptions
                .Setup(x => x.Value)
                .Returns(options);

            _subject = new TokenService(mockTokenOptions.Object);
        }

        [Fact(DisplayName = "GenerateTokenAsync should create a JWT token")]
        public void GenerateTokenAsync_ShouldReturnAString()
        {
            // Act
            var result = _subject.GenerateTokenAsync(_user);

            // Assert
            Assert.IsType<string>(result);
        }

        [Fact(DisplayName = "The returned JWT token should contain the expectedAudience")]
        public void GenerateTokenAsync_TokenShouldContainExpectedAudience()
        {
            // Act
            var result = _subject.GenerateTokenAsync(_user);
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result);

            // Assert
            Assert.Contains(jwtSecurityToken.Audiences, x => x == expectedAudience);
        }

        [Fact(DisplayName = "The returned JWT token should contain the expectedIssuer")]
        public void GenerateTokenAsync_TokenShouldContainExpectedIssuer()
        {
            // Act
            var result = _subject.GenerateTokenAsync(_user);
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result);

            // Assert
            Assert.True(jwtSecurityToken.Issuer == expectedIssuer);
        }
    }
}
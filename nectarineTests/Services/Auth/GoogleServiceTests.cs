using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NectarineAPI.Models;
using NectarineAPI.Services.Auth;
using Xunit;

namespace NectarineTests.Services.Auth
{
    public class GoogleServiceTests
    {
        private readonly GoogleAuthService<GoogleUser> _subject;

        public GoogleServiceTests()
        {
           // HttpClient setup
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(x =>
                        x.RequestUri!.Equals(
                            new Uri("https://www.googleapis.com/oauth2/v2/userinfo?access_token=token"))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\r\n  \"id\": \"12313123123\",\r\n" +
                                                "  \"name\": \"Jim Bob\",\r\n" +
                                                "  \"given_name\": \"Jim\",\r\n" +
                                                "  \"family_name\": \"Bob\"\r\n}"),
                });

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(x =>
                        x.RequestUri!.Equals(
                            new Uri("https://www.googleapis.com/oauth2/v2/userinfo?access_token=BADTOKEN"))),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Subject setup
            _subject = new GoogleAuthService<GoogleUser>(httpClient);
        }

        [Fact(DisplayName = "GetUserFromTokenAsync should return a GoogleUser")]
        public async Task Test_GetUserFromTokenAsync_ReturnsGoogleUser()
        {
            // Act
            var result = await _subject.GetUserFromTokenAsync("token");

            // Assert
            Assert.IsType<GoogleUser>(result);
        }

        [Fact(DisplayName = "GetUserFromTokenAsync should return null")]
        public async Task Test_GetUserFromTokenAsync_ReturnsNullWhen_InvalidGoogleUser()
        {
            // Act
            var result = await _subject.GetUserFromTokenAsync("BADTOKEN");

            // Assert
            Assert.Null(result);
        }
    }
}
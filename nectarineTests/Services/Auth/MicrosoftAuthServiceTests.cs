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

namespace NectarineTests.Services.Auth;

public class MicrosoftAuthServiceTests
{
    private MicrosoftAuthService<MicrosoftUser> _subject;

    public MicrosoftAuthServiceTests()
    {
        // HttpClient setup
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.RequestUri!.Equals(new Uri("https://graph.microsoft.com/v1.0/me"))),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\r\n  \"id\": \"12313123123\",\r\n" +
                                            "  \"name\": \"Jim Bob\",\r\n" +
                                            "  \"givenName\": \"Jim\",\r\n" +
                                            "  \"surname\": \"Bob\"\r\n}"),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Subject setup
        _subject = new MicrosoftAuthService<MicrosoftUser>(httpClient);
    }

    [Fact(DisplayName = "GetUserFromTokenAsync should return a MicrosoftUser")]
    public async Task Test_GetUserFromTokenAsync_Returns_MicrosoftUser()
    {
        // Act
        var result = await _subject.GetUserFromTokenAsync("token");

        // Assert
        Assert.IsType<MicrosoftUser>(result);
    }

    [Fact(DisplayName = "GetUserFromTokenAsync should return null")]
    public async Task Test_GetUserFromTokenAsync_ReturnsNullWhen_InvalidGoogleUser()
    {
        // Arrange
        // HttpClient setup
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.RequestUri!.Equals(new Uri("https://graph.microsoft.com/v1.0/me"))),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Subject setup
        _subject = new MicrosoftAuthService<MicrosoftUser>(httpClient);

        // Act
        var result = await _subject.GetUserFromTokenAsync("BADTOKEN");

        // Assert
        Assert.Null(result);
    }
}
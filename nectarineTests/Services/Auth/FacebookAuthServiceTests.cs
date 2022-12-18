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

public class FacebookAuthServiceTests
{
    private readonly FacebookAuthService<FacebookUser> _subject;

    public FacebookAuthServiceTests()
    {
        // HttpClient setup
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.RequestUri!.Equals(
                        new Uri("https://graph.facebook.com/v12.0/me?fields=id,name,email,first_name,last_name&access_token=token"))),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\r\n  \"id\": \"12313123123\",\r\n" +
                                            "  \"name\": \"Jim Bob\",\r\n" +
                                            "  \"first_name\": \"Jim\",\r\n" +
                                            "  \"last_name\": \"Bob\"\r\n}"),
            });

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x =>
                    x.RequestUri!.Equals(
                        new Uri("https://graph.facebook.com/v12.0/me?fields=id,name,email,first_name,last_name&access_token=BADTOKEN"))),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Subject setup
        _subject = new FacebookAuthService<FacebookUser>(httpClient);
    }

    [Fact(DisplayName = "GetUserFromTokenAsync should return a FacebookUser")]
    public async Task Test_GetUserFromTokenAsync_Returns_FacebookUser()
    {
        // Act
        var result = await _subject.GetUserFromTokenAsync("token");

        // Assert
        Assert.IsType<FacebookUser>(result);
    }

    [Fact(DisplayName = "GetUserFromTokenAsync should return null")]
    public async Task Test_GetUserFromTokenAsync_ReturnsNullWhen_InvalidFacebookUser()
    {
        // Act
        var result = await _subject.GetUserFromTokenAsync("BADTOKEN");

        // Assert
        Assert.Null(result);
    }
}
using System.Threading.Tasks;
using nectarineAPI.Models;
using nectarineAPI.Services.Auth;
using Xunit;

namespace nectarineTests.Services.Auth;

public class FacebookAuthServiceTests
{
    private readonly FacebookAuthService<FacebookUser> _subject = new ();

    // TODO: - Access Token Tests aren't going to work without real tokens.
    // [Fact(DisplayName = "GetUserFromTokenAsync should return a FacebookUser")]
    // public async Task Test_GetUserFromTokenAsync_Returns_FacebookUser()
    // {
    //     // Act
    //     var result = await _subject.GetUserFromTokenAsync("A Facebook token");
    //     
    //     // Assert
    //     Assert.IsType<FacebookUser>(result);
    // }
    
    [Fact(DisplayName = "GetUserFromTokenAsync should return null")]
    public async Task Test_GetUserFromTokenAsync_ReturnsNullWhen_InvalidFacebookUser()
    {
        // Act
        var result = await _subject.GetUserFromTokenAsync("token");

        // Assert
        Assert.Null(result);
    }
}
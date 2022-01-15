using System.Threading.Tasks;
using nectarineAPI.Models;
using nectarineAPI.Services.Auth;
using Xunit;

namespace nectarineTests.Services.Auth;

public class MicrosoftAuthServiceTests
{
    private readonly MicrosoftAuthService<MicrosoftUser> _subject = new ();

    // TODO: - Access Token Tests aren't going to work without real tokens.
    // [Fact(DisplayName = "GetUserFromTokenAsync should return a MicrosoftUser")]
    // public async Task Test_GetUserFromTokenAsync_Returns_MicrosoftUser()
    // {
    //     // Act
    //     var result = await _subject.GetUserFromTokenAsync("A mock microsoft token?");
    //
    //     // Assert
    //     Assert.IsType<MicrosoftUser>(result);
    // }

    [Fact(DisplayName = "GetUserFromTokenAsync should return null")]
    public async Task Test_GetUserFromTokenAsync_ReturnsNullWhen_InvalidGoogleUser()
    {
        // Act
        var result = await _subject.GetUserFromTokenAsync("token");

        // Assert
        Assert.Null(result);
    }
}
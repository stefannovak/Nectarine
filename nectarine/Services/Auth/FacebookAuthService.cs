using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using nectarineAPI.Models;

namespace nectarineAPI.Services.Auth;

public class FacebookAuthService<T> : IExternalAuthService<FacebookUser> where T : FacebookUser, new ()
{
    private HttpClient Client { get; }
        
    public FacebookAuthService()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("https://graph.facebook.com/v12.0/"),
        };
    }
    
    public async Task<FacebookUser?> GetUserFromTokenAsync(string token)
    {
        var response = await Client.GetAsync($"me?fields=id,name,email,first_name,last_name&access_token={token}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<T>(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        });
        return user;
    }
}
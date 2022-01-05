using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using nectarineAPI.Models;

namespace nectarineAPI.Services
{
    public class GoogleService<T> : ISocialService<GoogleUser> where T : GoogleUser, new ()
    {
        private HttpClient Client { get; set; }
        
        public GoogleService()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com/oauth2/v2/"),
            };
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<GoogleUser?> GetUserFromTokenAsync(string token)
        {
            var response = await Client.GetAsync(Uri.EscapeUriString($"userinfo?access_token={token}"));
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
}
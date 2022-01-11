using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using nectarineAPI.Models;

namespace nectarineAPI.Services
{
    public class MicrosoftAuthService<T> : IExternalAuthService<MicrosoftUser> where T : MicrosoftUser, new ()
    {
        public async Task<MicrosoftUser?> GetUserFromTokenAsync(string token)
        {
            var client = new HttpClient
            {
                DefaultRequestHeaders =
                {
                    Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}")
                }
            };

           var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
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
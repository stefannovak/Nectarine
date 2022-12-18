using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NectarineAPI.Models;

namespace NectarineAPI.Services.Auth
{
    public class MicrosoftAuthService<T> : IExternalAuthService<MicrosoftUser>
        where T : MicrosoftUser, new()
    {
        private readonly HttpClient _client;

        public MicrosoftAuthService(HttpClient client)
        {
            _client = client;
        }

        public async Task<MicrosoftUser?> GetUserFromTokenAsync(string token)
        {
            var message = new HttpRequestMessage();
            message.Headers.Authorization = new AuthenticationHeaderValue(token);
            message.RequestUri = new Uri("https://graph.microsoft.com/v1.0/me");

            var response = await _client.SendAsync(message);
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
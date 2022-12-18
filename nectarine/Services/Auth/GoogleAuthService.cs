using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NectarineAPI.Models;

namespace NectarineAPI.Services.Auth
{
    public class GoogleAuthService<T> : IExternalAuthService<GoogleUser>
        where T : GoogleUser, new()
    {
        private readonly HttpClient _httpClient;

        public GoogleAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://www.googleapis.com/oauth2/v2/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<GoogleUser?> GetUserFromTokenAsync(string token)
        {
            var response = await _httpClient.GetAsync($"userinfo?access_token={token}");
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
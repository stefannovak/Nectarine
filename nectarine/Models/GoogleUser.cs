using System.Text.Json.Serialization;
using NectarineData.Models.Enums;

namespace NectarineAPI.Models
{
    public class GoogleUser : IExternalAuthUser
    {
        public string? Id { get; set; }

        public string? Platform { get; set; }

        [JsonPropertyName("given_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("family_name")]
        public string? LastName { get; set; }

        public string? Email { get; set; }
    }
}
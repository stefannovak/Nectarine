using System.Text.Json.Serialization;
using nectarineData.Models.Enums;

namespace nectarineAPI.Models
{
    public class GoogleUser : ISocialUser
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
using System.Text.Json.Serialization;

namespace nectarineAPI.Models
{
    public class MicrosoftUser : IExternalAuthUser
    {
        public string? Id { get; set; }

        public string? Platform { get; set; }

        [JsonPropertyName("givenName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("surname")]
        public string? LastName { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string? Email { get; set; }
    }   
}
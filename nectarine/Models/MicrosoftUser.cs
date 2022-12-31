using System.Text.Json.Serialization;

namespace NectarineAPI.Models
{
    public class MicrosoftUser : IExternalAuthUser
    {
        public MicrosoftUser()
        {
        }

        public MicrosoftUser(string id, string firstName, string lastName, string email)
        {
            Id = id;
            Platform = "microsoft";
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public string Id { get; set; }

        public string Platform { get; set; }

        [JsonPropertyName("givenName")]
        public string FirstName { get; set; }

        [JsonPropertyName("surname")]
        public string LastName { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string Email { get; set; }
    }
}
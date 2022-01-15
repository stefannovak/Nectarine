using System.Text.Json.Serialization;

namespace NectarineAPI.Models;

public class FacebookUser : IExternalAuthUser
{
    public string? Id { get; set; }

    public string? Platform { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    public string? Email { get; set; }
}
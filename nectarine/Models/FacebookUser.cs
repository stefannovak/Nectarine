using System.Text.Json.Serialization;

namespace NectarineAPI.Models;

public class FacebookUser : IExternalAuthUser
{
    public FacebookUser()
    {
    }

    public FacebookUser(string id, string firstName, string lastName, string email)
    {
        Id = id;
        Platform = "facebook";
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public string Id { get; set; }

    public string Platform { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    public string Email { get; set; }
}
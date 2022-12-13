namespace NectarineAPI.DTOs.Responses
{
    public class CreateUserResponse
    {
        public CreateUserResponse(string token)
        {
            Token = token;
        }

        public string Token { get; set; }
    }
}
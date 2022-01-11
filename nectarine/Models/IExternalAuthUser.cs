namespace nectarineAPI.Models
{
    public interface IExternalAuthUser
    {
        public string? Id { get; set; }

        public string? Platform { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
    }
}
namespace NectarineAPI.Models
{
    public interface IExternalAuthUser
    {
        // TODO: -  Do these need to be nullable???
        public string Id { get; set; }

        public string Platform { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
    }
}
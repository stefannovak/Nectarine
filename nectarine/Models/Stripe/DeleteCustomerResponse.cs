namespace nectarineAPI.Models.Stripe
{
    /// <summary>
    /// <see>
    ///     <cref>https://stripe.com/docs/api/customers/delete?lang=dotnet</cref>
    /// </see>
    /// </summary>
    public class DeleteCustomerResponse
    {
        public string? Id { get; set; }

        public string? Object { get; set; }

        public bool? Deleted { get; set; }
    }
}
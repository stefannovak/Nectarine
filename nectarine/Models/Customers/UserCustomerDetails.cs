namespace NectarineAPI.Models.Customers;

public class UserCustomerDetails
{
    public UserCustomerDetails(
        string customerId,
        string defaultPaymentSourceId,
        string email,
        string name,
        long balance)
    {
        CustomerId = customerId;
        DefaultPaymentSourceId = defaultPaymentSourceId;
        Email = email;
        Name = name;
        Balance = balance;
    }

    public string CustomerId { get; set; }

    public string DefaultPaymentSourceId { get; set; }

    public string Email { get; set; }

    public string Name { get; set; }

    public long Balance { get; set; }    
}
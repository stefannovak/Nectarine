namespace NectarineAPI.Models.Customers;

public class UserCustomerDetails
{
    public UserCustomerDetails(
        string customerId,
        string? defaultPaymentSourceId,
        string? email,
        string? phoneNumber,
        string? name,
        long balance,
        UserAddress? address)
    {
        CustomerId = customerId;
        DefaultPaymentSourceId = defaultPaymentSourceId;
        Email = email;
        PhoneNumber = phoneNumber;
        Name = name;
        Balance = balance;
        Address = address;
    }

    public string CustomerId { get; set; }

    public string? DefaultPaymentSourceId { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Name { get; set; }

    public long Balance { get; set; }

    public UserAddress? Address { get; set; }
}
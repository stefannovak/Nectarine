namespace NectarineAPI.Models.Payments;

public class SensitivePaymentMethod
{
    public SensitivePaymentMethod(string id, string customerId, long expiryMonth, long expiryYear, string lastFour)
    {
        Id = id;
        CustomerId = customerId;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        LastFour = lastFour;
    }

    public string Id { get; set; }

    public string CustomerId { get; set; }

    public long ExpiryMonth { get; set; }

    public long ExpiryYear { get; set; }

    public string LastFour { get; set; }
}
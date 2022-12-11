namespace NectarineAPI.Models.Payments;

/// <summary>
/// A representation of a payment card that doesn't expose sensitive data.
/// </summary>
public class InsensitivePaymentMethod
{
    public InsensitivePaymentMethod(long expiryMonth, long expiryYear, string lastFour)
    {
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        LastFour = lastFour;
    }

    public long ExpiryMonth { get; set; }

    public long ExpiryYear { get; set; }

    public string LastFour { get; set; }
}
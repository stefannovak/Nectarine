using System;

namespace NectarineAPI.Models.Payments;

public class PaymentIntentResponse
{
    public PaymentIntentResponse(
        string paymentIntentId,
        long amount,
        DateTime createdAt,
        string currency,
        string status,
        string clientSecret)
    {
        PaymentIntentId = paymentIntentId;
        Amount = amount;
        CreatedAt = createdAt;
        Currency = currency;
        Status = status;
        ClientSecret = clientSecret;
    }

    public string PaymentIntentId { get; set; }

    /// <summary>
    /// 100 = £1.00
    /// </summary>
    public long Amount { get; set; }

    public DateTime CreatedAt{ get; set; }

    public string Currency { get; set; }

    public string Status { get; set; }

    public string ClientSecret { get; set; }
}
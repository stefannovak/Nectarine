using Stripe;

namespace nectarineAPI.Services
{
    public class StripeService : IStripeService
    {
        public bool TestStripeMethod()
        {
            var options = new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = "4242424242424242",
                    ExpMonth = 8,
                    ExpYear = 2022,
                    Cvc = "314",
                },
            };
            var service = new PaymentMethodService();
            var paymentMethod = service.Create(options);
            return paymentMethod != null;
        }
    }
}
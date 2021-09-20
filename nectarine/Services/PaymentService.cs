using System;
using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Services
{
    public class PaymentService : IPaymentService
    { 
        private PaymentMethodService PaymentMethodService { get; } = new ();
        private PaymentIntentService PaymentIntentService { get; } = new ();

        public StripeException? AddCardPaymentMethod(
            ApplicationUser user,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc)
        {
            var paymentMethodCreateOptions = new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = cardNumber,
                    ExpMonth = expiryMonth,
                    ExpYear = expiryYear,
                    Cvc = cvc,
                },
            };

            PaymentMethod? paymentMethod;
            try
            {
                paymentMethod = PaymentMethodService.Create(paymentMethodCreateOptions);
            }
            catch (StripeException e)
            {
                Console.WriteLine(e);
                return e;
            }
            
            var paymentMethodAttachOptions = new PaymentMethodAttachOptions
            {
                Customer = user.StripeCustomerId,
            };
            
            PaymentMethodService.Attach(
                paymentMethod.Id,
                paymentMethodAttachOptions
            );

            return null;
        }

        public StripeList<PaymentMethod> GetCardsForUser(ApplicationUser user)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = user.StripeCustomerId,
                Type = "card",
            };

            return PaymentMethodService.List(
                options
            );
        }

        public PaymentIntent CreatePaymentIntent(ApplicationUser user, long amount, string paymentMethodId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Customer = user.StripeCustomerId,
                PaymentMethod = paymentMethodId,
                SetupFutureUsage = "on_session"
            };
            
            return PaymentIntentService.Create(options);
        }

        public PaymentIntent ConfirmPaymentIntent(string paymentIntentClientSecret) => 
            PaymentIntentService.Confirm(paymentIntentClientSecret);
    }
}
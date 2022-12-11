using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NectarineData.Models;
using Stripe;

namespace NectarineAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            ILogger<PaymentService> logger)
        {
            PaymentMethodService = new PaymentMethodService();
            PaymentIntentService = new PaymentIntentService();
            _logger = logger;
        }

        public PaymentMethodService PaymentMethodService { get; init; }

        public PaymentIntentService PaymentIntentService { get; init; }

        public StripeException? AddCardPaymentMethod(
            string stripeCustomerId,
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
                _logger.LogError($"Failed to create a payment method for the user: {e}");
                return e;
            }

            var paymentMethodAttachOptions = new PaymentMethodAttachOptions
            {
                Customer = stripeCustomerId,
            };

            PaymentMethodService.Attach(
                paymentMethod.Id,
                paymentMethodAttachOptions);

            return null;
        }

        public IEnumerable<PaymentMethod> GetCardsForUser(ApplicationUser user)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = user.StripeCustomerId,
                Type = "card",
            };

            return PaymentMethodService.List(options);
        }

        public PaymentMethod? GetPaymentMethod(string id) => PaymentMethodService.Get(id);

        public PaymentIntent CreatePaymentIntent(ApplicationUser user, long amount, string paymentMethodId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Customer = user.StripeCustomerId,
                PaymentMethod = paymentMethodId,
                SetupFutureUsage = "on_session",
            };

            return PaymentIntentService.Create(options);
        }

        public PaymentIntent ConfirmPaymentIntent(string paymentIntentClientSecret) =>
            PaymentIntentService.Confirm(paymentIntentClientSecret);
    }
}
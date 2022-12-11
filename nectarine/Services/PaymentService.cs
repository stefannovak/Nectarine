using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NectarineAPI.Models.Payments;
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

        public bool AddCardPaymentMethod(
            string paymentProviderCustomerId,
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
                return false;
            }

            var paymentMethodAttachOptions = new PaymentMethodAttachOptions
            {
                Customer = paymentProviderCustomerId,
            };

            PaymentMethodService.Attach(
                paymentMethod.Id,
                paymentMethodAttachOptions);

            return true;
        }

        public IEnumerable<InsensitivePaymentCard> GetCardsForUser(string paymentProviderCustomerId)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = paymentProviderCustomerId,
                Type = "card",
            };

            var paymentMethods = PaymentMethodService.List(options);
            var cards =
                paymentMethods.Data
                    .Where(x => x.Card is not null)
                    .Select(paymentMethod =>
                        new InsensitivePaymentCard(
                            paymentMethod.Card.ExpMonth,
                            paymentMethod.Card.ExpYear,
                            paymentMethod.Card.Last4));

            return cards;
        }

        public SensitivePaymentMethod? GetPaymentMethod(string id)
        {
            var paymentMethod = PaymentMethodService.Get(id);
            return paymentMethod == null
                ? null
                : new SensitivePaymentMethod(
                    paymentMethod.Id,
                    paymentMethod.CustomerId,
                    paymentMethod.Card.ExpMonth,
                    paymentMethod.Card.ExpYear,
                    paymentMethod.Card.Last4);
        }

        public PaymentIntent CreatePaymentIntent(string paymentProviderCustomerId, long amount, string paymentMethodId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Customer = paymentProviderCustomerId,
                PaymentMethod = paymentMethodId,
                SetupFutureUsage = "on_session",
            };

            return PaymentIntentService.Create(options);
        }

        public PaymentIntent ConfirmPaymentIntent(string paymentIntentClientSecret) =>
            PaymentIntentService.Confirm(paymentIntentClientSecret);
    }
}
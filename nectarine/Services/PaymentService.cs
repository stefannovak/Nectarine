using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NectarineAPI.Models.Payments;
using NectarineData.Models;
using Serilog;
using Stripe;

namespace NectarineAPI.Services
{
    public class PaymentService : IPaymentService
    {
        public PaymentService()
        {
            PaymentMethodService = new PaymentMethodService();
            PaymentIntentService = new PaymentIntentService();
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
                Log.Error($"Failed to create a payment method for the user: {e}");
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

        public IEnumerable<InsensitivePaymentMethod> GetCardsForUser(string paymentProviderCustomerId)
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
                        new InsensitivePaymentMethod(
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

        public PaymentIntentResponse? CreatePaymentIntent(
            string paymentProviderCustomerId,
            long amount,
            string paymentMethodId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                Customer = paymentProviderCustomerId,
                PaymentMethod = paymentMethodId,
                SetupFutureUsage = "on_session",
            };

            var paymentIntent = PaymentIntentService.Create(options);
            return paymentIntent == null
                ? null
                : new PaymentIntentResponse(
                    paymentIntent.Id,
                    paymentIntent.Amount,
                    paymentIntent.Created,
                    paymentIntent.Currency,
                    paymentIntent.Status,
                    paymentIntent.ClientSecret);
        }

        public PaymentIntentResponse? ConfirmPaymentIntent(string paymentIntentClientSecret)
        {
            var paymentIntent = PaymentIntentService.Confirm(paymentIntentClientSecret);
            return paymentIntent == null
                ? null
                : new PaymentIntentResponse(
                    paymentIntent.Id,
                    paymentIntent.Amount,
                    paymentIntent.Created,
                    paymentIntent.Currency,
                    paymentIntent.Status,
                    paymentIntent.ClientSecret);
        }

    }
}
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NectarineData.Models;
using Stripe;

namespace NectarineAPI.Services
{
    public class PaymentService : IPaymentService
    {
        public PaymentMethodService _paymentMethodService;
        public PaymentIntentService _paymentIntentService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            // PaymentMethodService paymentMethodService,
            // PaymentIntentService paymentIntentService,
            ILogger<PaymentService> logger)
        {
            // _paymentMethodService = paymentMethodService;
            // _paymentIntentService = paymentIntentService;
            _paymentMethodService = new PaymentMethodService();
            _paymentIntentService = new PaymentIntentService();
            _logger = logger;
        }

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
                paymentMethod = _paymentMethodService.Create(paymentMethodCreateOptions);
            }
            catch (StripeException e)
            {
                _logger.LogError($"Failed to create a payment method for the user: {e}");
                return e;
            }

            var paymentMethodAttachOptions = new PaymentMethodAttachOptions
            {
                Customer = user.StripeCustomerId,
            };

            _paymentMethodService.Attach(
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

            return _paymentMethodService.List(options);
        }

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

            return _paymentIntentService.Create(options);
        }

        public PaymentIntent ConfirmPaymentIntent(string paymentIntentClientSecret) =>
            _paymentIntentService.Confirm(paymentIntentClientSecret);
    }
}
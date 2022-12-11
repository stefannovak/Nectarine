using System.Collections.Generic;
using NectarineData.Models;
using Stripe;

namespace NectarineAPI.Services
{
    // TODO: - Do not return Stripe classes in an interface
    public interface IPaymentService
    {
        public StripeException? AddCardPaymentMethod(
            string stripeCustomerId,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc);

        public IEnumerable<PaymentMethod> GetCardsForUser(ApplicationUser user);

        public PaymentMethod? GetPaymentMethod(string id);

        /// <summary>
        /// Creates a <see cref="PaymentIntent"/> object which attaches the user to it. It should be used at the start
        /// of the checkout process, and updated throughout.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="amount">The amount to charge in the smallest currency unit. (100 = 100p in GBP).</param>
        /// <param name="paymentMethodId">The selected payment method from the user to charge.</param>
        /// <returns><see cref="PaymentIntent"/>. The ClientSecret parameter should be passed back to the client.</returns>
        public PaymentIntent CreatePaymentIntent(ApplicationUser user, long amount, string paymentMethodId);

        /// <summary>
        /// Confirms a <see cref="PaymentIntent"/>.
        /// </summary>
        /// <param name="paymentIntentClientSecret"></param>
        /// <returns>Returns an updated <see cref="PaymentIntent"/>.</returns>
        public PaymentIntent ConfirmPaymentIntent(string paymentIntentClientSecret);
    }
}
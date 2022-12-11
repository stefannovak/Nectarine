using System.Collections.Generic;
using NectarineAPI.Models.Payments;

namespace NectarineAPI.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Attaches a payment carCd to the user.
        /// </summary>
        /// <param name="paymentProviderCustomerId">The Payment Provider ID of a user.</param>
        /// <param name="cardNumber">16 digit card number.</param>
        /// <param name="expiryMonth">2 digit expiry month.</param>
        /// <param name="expiryYear">2 digit expiry year.</param>
        /// <param name="cvc">3-4 digit security code.</param>
        /// <returns>Whether the card was successfully added or not.</returns>
        public bool AddCardPaymentMethod(
            string paymentProviderCustomerId,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc);

        /// <summary>
        /// Get a list of visa type cards for a user.
        /// </summary>
        /// <param name="paymentProviderCustomerId"></param>
        /// <returns>A list of expiry months, years and last 4 digits.</returns>
        public IEnumerable<InsensitivePaymentCard> GetCardsForUser(string paymentProviderCustomerId);

        /// <summary>
        /// Get a visa type card by ID.
        /// </summary>
        /// <param name="id">Payment Card ID.</param>
        /// <returns></returns>
        public SensitivePaymentMethod? GetPaymentMethod(string id);

        /// <summary>
        /// Creates a PaymentIntent object which attaches the user to it. It should be used at the start
        /// of the checkout process, and updated throughout.
        /// </summary>
        /// <param name="paymentProviderCustomerId"></param>
        /// <param name="amount">The amount to charge in the smallest currency unit. (100 = 100p in GBP).</param>
        /// <param name="paymentMethodId">The selected payment method from the user to charge.</param>
        /// <returns><see cref="PaymentIntentResponse"/>. The ClientSecret parameter should be passed back to the client.</returns>
        public PaymentIntentResponse? CreatePaymentIntent(
            string paymentProviderCustomerId,
            long amount,
            string paymentMethodId);

        /// <summary>
        /// Confirm a PaymentIntent. This will bill the user.
        /// </summary>
        /// <param name="paymentIntentClientSecret">A value given by the client, from a previous payment intent.</param>
        /// <returns>Returns an updated <see cref="PaymentIntentResponse"/>.</returns>
        public PaymentIntentResponse? ConfirmPaymentIntent(string paymentIntentClientSecret);
    }
}
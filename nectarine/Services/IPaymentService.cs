using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Adds a card <see cref="PaymentMethod"/> to the users <see cref="Customer"/> object.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cardNumber">The long card number</param>
        /// <param name="expiryMonth">The expiry month as an integer</param>
        /// <param name="expiryYear">The expiry year</param>
        /// <param name="cvc"></param>
        public StripeException? AddCardPaymentMethod(
            ApplicationUser user,   
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc);

        /// <summary>
        /// Gets a list of <see cref="PaymentMethod"/>s from the users <see cref="Customer"/> object.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A list of Payment Methods for the user</returns>
        public StripeList<PaymentMethod> GetCardsForUser(ApplicationUser user);

        /// <summary>
        /// Creates a <see cref="PaymentIntent"/> object which attaches the user to it. It should be used at the start
        /// of the checkout process, and updated throughout.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="amount">The amount to charge in the smallest currency unit. (100 = 100p in GBP)</param>
        /// <param name="paymentMethodId">The selected payment method from the user to charge</param>
        /// <returns><see cref="PaymentIntent"/>. The ClientSecret parameter should be passed back to the client</returns>
        public PaymentIntent CreatePaymentIntent(ApplicationUser user, long amount, string paymentMethodId);

        /// <summary>
        /// Confirms a <see cref="PaymentIntent"/>.
        /// </summary>
        /// <param name="paymentIntentClientSecret"></param>
        /// <returns>Returns an updated <see cref="PaymentIntent"/></returns>
        public PaymentIntent ConfirmPaymentIntent(string paymentIntentClientSecret);
    }
}
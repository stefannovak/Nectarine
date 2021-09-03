using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public bool AddCardPaymentMethod(
            ApplicationUser user,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc);

        /// <summary>
        /// Gets a list of <see cref="PaymentMethod"/>s from the users <see cref="Customer"/> object.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public StripeList<PaymentMethod> GetCardsForUser(ApplicationUser user);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Services
{
    public interface IPaymentService
    {
        public Task<bool> AddStripeCustomerIdAsync(ApplicationUser user);
        
        /// <summary>
        /// Adds a payment card ID to the user's list of [PaymentMethodIds].
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cardNumber">The long card number</param>
        /// <param name="expiryMonth">The expiry month as an integer</param>
        /// <param name="expiryYear">The expiry year</param>
        /// <param name="cvc"></param>
        public Task AddCardToAccountAsync(
            ApplicationUser user,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc);

        public StripeList<Card> GetCardsForUser(ApplicationUser user);
    }
}
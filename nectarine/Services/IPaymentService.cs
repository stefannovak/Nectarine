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
        /// Attaches a StripeCustomerId to the user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Returns </returns>
        public Task AddStripeCustomerIdAsync(ApplicationUser user);

        /// <summary>
        /// Gets the customers [Customer] object from their customer ID.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Customer GetCustomer(ApplicationUser user);

        /// <summary>
        /// Updates the users [Customer] object.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="updateOptions"></param>
        /// <returns></returns>
        public Customer UpdateCustomer(ApplicationUser user, CustomerUpdateOptions updateOptions);
        
        /// <summary>
        /// Adds a card Payment Method to the users Customer object.
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
        /// Gets the users Payment Methods of type Card
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public StripeList<PaymentMethod> GetCardsForUser(ApplicationUser user);
    }
}
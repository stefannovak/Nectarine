using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using nectarineData.DataAccess;
using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly NectarineDbContext _context;

        public PaymentService(NectarineDbContext context)
        {
            _context = context;
        }
        
        public async Task AddCardToAccountAsync(
            ApplicationUser user,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvc)
        {
            var options = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = cardNumber,
                    ExpMonth = expiryMonth,
                    ExpYear = expiryYear,
                    Cvc = cvc,
                }
            };

            var service = new TokenService();
            try
            {
                var cardToken = service.Create(options);
                user.PaymentMethodIds.Add(new PaymentMethodId
                {
                    TokenId = cardToken.Id
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public StripeList<Card> GetCardsForUser(ApplicationUser user)
        {
            var service = new CardService();
            
            var cards = service.List(user.StripeCustomerId);

            return cards;
        }
    }
}
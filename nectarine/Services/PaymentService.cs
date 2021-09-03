using System.Threading.Tasks;
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

        private PaymentMethodService PaymentMethodService { get; } = new ();
        private CustomerService CustomerService { get; } = new ();
        
        public async Task AddStripeCustomerIdAsync(ApplicationUser user)
        {
            var options = new CustomerCreateOptions
            {
                Description = "",
            };
            var customer = CustomerService.Create(options);

            user.StripeCustomerId = customer.Id;
            await _context.SaveChangesAsync();
        }

        public Customer GetCustomer(ApplicationUser user) => CustomerService.Get(user.StripeCustomerId);


        public bool AddCardPaymentMethod(
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
            
            var paymentMethod = PaymentMethodService.Create(paymentMethodCreateOptions);
            var paymentMethodAttachOptions = new PaymentMethodAttachOptions
            {
                Customer = user.StripeCustomerId,
            };
            
            PaymentMethodService.Attach(
                paymentMethod.Id,
                paymentMethodAttachOptions
            );
            
            return true;
        }

        public StripeList<PaymentMethod> GetCardsForUser(ApplicationUser user)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = user.StripeCustomerId,
                Type = "card",
            };

            return PaymentMethodService.List(
                options
            );
        }
    }
}
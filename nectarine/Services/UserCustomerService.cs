using System;
using System.Threading.Tasks;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;

namespace NectarineAPI.Services
{
    public class UserCustomerService : IUserCustomerService
    {
        private readonly NectarineDbContext _context;
        private readonly CustomerService _customerService;

        public UserCustomerService(NectarineDbContext context, CustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        public async Task AddStripeCustomerIdAsync(ApplicationUser user, CustomerCreateOptions? options = null)
        {
            var customer = await _customerService.CreateAsync(options ?? new CustomerCreateOptions());
            user.StripeCustomerId = customer.Id;
            await _context.SaveChangesAsync();
        }

        public Customer GetCustomer(ApplicationUser user) => _customerService.Get(user.StripeCustomerId);

        public Customer UpdateCustomer(ApplicationUser user, CustomerUpdateOptions updateOptions) =>
            _customerService.Update(user.StripeCustomerId, updateOptions);

        public bool DeleteCustomer(ApplicationUser user)
        {
            try
            {
                var customer = _customerService.Delete(user.StripeCustomerId);
                return customer?.Deleted == true;
            }
            catch (StripeException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
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

        public UserCustomerService(NectarineDbContext context)
        {
            _context = context;
            CustomerService = new CustomerService();
        }

        public CustomerService CustomerService { get; init; }

        public async Task AddStripeCustomerIdAsync(ApplicationUser user, CustomerCreateOptions? options = null)
        {
            var customer = await CustomerService.CreateAsync(options ?? new CustomerCreateOptions());
            user.StripeCustomerId = customer.Id;
            await _context.SaveChangesAsync();
        }

        public Customer GetCustomer(ApplicationUser user) => CustomerService.Get(user.StripeCustomerId);

        public Customer UpdateCustomer(ApplicationUser user, CustomerUpdateOptions updateOptions) =>
            CustomerService.Update(user.StripeCustomerId, updateOptions);

        public bool DeleteCustomer(ApplicationUser user)
        {
            try
            {
                var customer = CustomerService.Delete(user.StripeCustomerId);
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
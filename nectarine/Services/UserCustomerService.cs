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

        public async Task AddCustomerIdAsync(ApplicationUser user)
        {
            var customer = await CustomerService.CreateAsync(new CustomerCreateOptions());
            user.PaymentProviderCustomerId = customer.Id;
            await _context.SaveChangesAsync();
        }

        public Customer GetCustomer(ApplicationUser user) => CustomerService.Get(user.PaymentProviderCustomerId);

        public Customer UpdateCustomer(ApplicationUser user, CustomerUpdateOptions updateOptions) =>
            CustomerService.Update(user.PaymentProviderCustomerId, updateOptions);

        public bool DeleteCustomer(ApplicationUser user)
        {
            try
            {
                var customer = CustomerService.Delete(user.PaymentProviderCustomerId);
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
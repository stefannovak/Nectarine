using System.Threading.Tasks;
using nectarineData.DataAccess;
using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Services
{
    public class UserCustomerService : IUserCustomerService
    {
        private readonly NectarineDbContext _context;
        
        public UserCustomerService(NectarineDbContext context)
        {
            _context = context;
        }

        private CustomerService CustomerService { get; } = new ();
        
        public async Task AddStripeCustomerIdAsync(ApplicationUser user, CustomerCreateOptions? options = null)
        {
            var customer = CustomerService.Create(options ?? new CustomerCreateOptions());
            user.StripeCustomerId = customer.Id;
            await _context.SaveChangesAsync();
        }

        public Customer GetCustomer(ApplicationUser user) => CustomerService.Get(user.StripeCustomerId);
        
        public Customer UpdateCustomer(ApplicationUser user, CustomerUpdateOptions updateOptions) =>
            CustomerService.Update(user.StripeCustomerId, updateOptions);
    }
}
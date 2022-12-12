using System;
using System.Threading.Tasks;
using NectarineAPI.Models.Customers;
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

        public UserCustomerDetails? GetCustomer(string paymentProviderCustomerId)
        {
            var customer = CustomerService.Get(paymentProviderCustomerId);
            return customer == null
                ? null
                : new UserCustomerDetails(
                    customer.Id,
                    customer.DefaultSourceId,
                    customer.Email,
                    customer.Name,
                    customer.Balance,
                    new UserAddress(
                        customer.Address.Line1,
                        customer.Address.Line2,
                        customer.Address.City,
                        customer.Address.PostalCode,
                        customer.Address.Country,
                        true));
        }

        public UserCustomerDetails UpdateCustomerAddress(string paymentProviderCustomerId)
        {
            var customer = CustomerService.Update(paymentProviderCustomerId, new CustomerUpdateOptions());
            return new UserCustomerDetails(
                customer.Id,
                customer.DefaultSourceId,
                customer.Email,
                customer.Name,
                customer.Balance,
                new UserAddress(
                    customer.Address.Line1,
                    customer.Address.Line2,
                    customer.Address.City,
                    customer.Address.PostalCode,
                    customer.Address.Country,
                    true));
        }
        
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
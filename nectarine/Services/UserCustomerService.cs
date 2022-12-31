using System;
using System.Threading.Tasks;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.Models.Customers;
using NectarineData.DataAccess;
using NectarineData.Models;
using Serilog;
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
                : MapCustomer(customer);
        }

        public UserCustomerDetails? UpdateCustomerAddress(string paymentProviderCustomerId, UserAddressDTO address)
        {
            var customer = CustomerService.Update(paymentProviderCustomerId, new CustomerUpdateOptions
            {
                Address = new AddressOptions
                {
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    City = address.City,
                    PostalCode = address.Postcode,
                    Country = address.Country,
                },
            });

            return customer == null
                ? null
                : MapCustomer(customer);
        }

        public UserCustomerDetails? UpdateCustomerPhoneNumber(string paymentProviderCustomerId, string phoneNumber)
        {
            var customer = CustomerService.Update(paymentProviderCustomerId, new CustomerUpdateOptions
            {
                Phone = phoneNumber,
            });

            return customer == null
                ? null
                : MapCustomer(customer);
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
                Log.Error(e, "Error deleting Customer");
                return false;
            }
        }

        private static UserCustomerDetails MapCustomer(Customer customer)
        {
            var hasAddress = customer.Address?.Line1 != null &&
                             customer.Address?.Line2 != null &&
                             customer.Address?.City != null &&
                             customer.Address?.PostalCode != null &&
                             customer.Address?.Country != null;

            // Primary is super wrong
            var address = hasAddress
                ? new UserAddress(
                    customer.Address!.Line1,
                    customer.Address.Line2,
                    customer.Address.City,
                    customer.Address.PostalCode,
                    customer.Address.Country,
                    true)
                : null;

            return new UserCustomerDetails(
                customer.Id,
                customer.DefaultSourceId,
                customer.Email,
                customer.Phone,
                customer.Name,
                customer.Balance,
                hasAddress ? address : null);
        }
    }
}
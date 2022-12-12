using System.Threading.Tasks;
using NectarineAPI.Models.Customers;
using NectarineData.Models;
using Stripe;

namespace NectarineAPI.Services
{
    public interface IUserCustomerService
    {
        /// <summary>
        /// Attach a CustomerId to a User.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task AddCustomerIdAsync(ApplicationUser user);

        /// <summary>
        /// Gets the users <see cref="Customer"/> object from their Customer ID.
        /// </summary>
        /// <param name="paymentProviderCustomerId"></param>
        /// <returns></returns>
        public UserCustomerDetails? GetCustomer(string paymentProviderCustomerId);

        /// <summary>
        /// Updates the users <see cref="Customer"/> object.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="updateOptions"></param>
        /// <returns></returns>
        public Customer UpdateCustomer(ApplicationUser user, CustomerUpdateOptions updateOptions);

        /// <summary>
        /// Sets the users <see cref="Customer"/> Deleted property to true.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool DeleteCustomer(ApplicationUser user);
    }
}
using System.Threading.Tasks;
using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Services
{
    public interface IUserCustomerService
    {
        /// <summary>
        /// Attaches a StripeCustomerId to the user and creates a <see cref="Customer"/>
        /// object with <see cref="CustomerCreateOptions"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="options"></param>
        /// <returns>Returns </returns>
        public Task AddStripeCustomerIdAsync(ApplicationUser user, CustomerCreateOptions? options = null);

        /// <summary>
        /// Gets the users <see cref="Customer"/> object from their Customer ID.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Customer GetCustomer(ApplicationUser user);

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
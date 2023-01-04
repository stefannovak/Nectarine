using System.Threading.Tasks;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.Models.Customers;
using NectarineData.Models;

namespace NectarineAPI.Services
{
    /// <summary>
    /// An interface that defines functions related to the User's CUSTOMER object, not the User itself.
    /// </summary>
    public interface IUserCustomerService
    {
        /// <summary>
        /// Attach a CustomerId to a User.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task AddCustomerIdAsync(ApplicationUser user);

        /// <summary>
        /// Gets a users customer details.
        /// </summary>
        /// <param name="paymentProviderCustomerId"></param>
        /// <returns></returns>
        public UserCustomerDetails? GetCustomer(string paymentProviderCustomerId);

        /// <summary>
        /// Sets a user to deleted. Does not delete the user object.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool DeleteCustomer(ApplicationUser user);
    }
}
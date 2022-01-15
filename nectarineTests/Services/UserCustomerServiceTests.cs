using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NectarineAPI.Services;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Services
{
    public class UserCustomerServiceTests
    {
        private readonly Mock<IConfigurationSection> _configurationSection = new ();
        private readonly ApplicationUser user = new ();
        private readonly UserCustomerService _userCustomerService;

        public UserCustomerServiceTests()
        {
            // Configuration setup
            _configurationSection.Setup(x => x.Path).Returns("Stripe");
            _configurationSection.Setup(x => x.Key).Returns("Secret");
            _configurationSection.Setup(x => x.Value).Returns("sk_test_26PHem9AhJZvU623DfE1x4sd");
            StripeConfiguration.ApiKey = _configurationSection.Object.Value;

            // Database setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            NectarineDbContext mockContext = new (options);
            _userCustomerService = new UserCustomerService(mockContext);
        }

        [Fact(DisplayName = "An API key should be correctly set up at 'Stripe:Secret' in IConfiguration")]
        public void Test_Configuration_ReturnsAnApiKey()
        {
            // Assert
            Assert.NotNull(StripeConfiguration.ApiKey);
        }

        #region Customers

        [Fact(DisplayName = "AddStripeCustomerIdAsync should save a StripeId to the User")]
        public async Task Test_AddStripeCustomerIdAsync()
        {
            // Arrange
            var customerCreateOptions = new CustomerCreateOptions
            {
                Email = "test@test.com",
            };

            // Act
            await _userCustomerService.AddStripeCustomerIdAsync(user, customerCreateOptions);

            // Assert
            Assert.False(user.StripeCustomerId.IsNullOrEmpty());
        }

        [Fact(DisplayName = "GetCustomer should fetch a customer object, filled with customer information.")]
        public async Task Test_GetCustomer()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);

            // Act
            var result = _userCustomerService.GetCustomer(user);

            // Arrange
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "UpdateCustomer should update the user's Customer object.")]
        public async Task Test_UpdateCustomer()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            var customerBeforeUpdate = _userCustomerService.GetCustomer(user);
            var updateOptions = new CustomerUpdateOptions
            {
                Balance = 100,
            };

            // Act
            var customerAfterUpdate = _userCustomerService.UpdateCustomer(user, updateOptions);

            // Assert
            Assert.NotEqual(customerBeforeUpdate.Balance, customerAfterUpdate.Balance);
        }

        [Fact(DisplayName = "DeleteCustomer should delete the users Customer object.")]
        public async Task Test_DeleteCustomer()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);

            // Act
            var result = _userCustomerService.DeleteCustomer(user);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "DeleteCustomer should return false when unable to delete a customer.")]
        public void Test_DeleteCustomer_FailsWhen_DeletingAUserWithoutACustomerObject()
        {
            // Arrange
            user.StripeCustomerId = "test";

            // Act
            var result = _userCustomerService.DeleteCustomer(user);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}

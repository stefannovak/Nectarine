using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Models.Customers;
using NectarineAPI.Services;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Services
{
    public class UserCustomerServiceTests
    {
        private readonly ApplicationUser user = new ()
        {
            PaymentProviderCustomerId = "StripeId",
        };

        private readonly UserCustomerService _userCustomerService;
        private readonly Mock<CustomerService> _mockCustomerService;

        public UserCustomerServiceTests()
        {
            // Stripe CustomerService setup
            _mockCustomerService = new Mock<CustomerService>();
            var returnedCustomer = new Customer
            {
                Id = "StripeCustomerId",
                DefaultSourceId = "pm",
                Email = "test@test.com",
                Name = "name",
                Balance = 123,
                Address = new Address
                {
                    Line1 = "21 BoolProp Lane",
                    Line2 = "test",
                    City = "Big City",
                    PostalCode = "N1111",
                    Country = "UK"
                }
            };

            _mockCustomerService
                .Setup(x => x.CreateAsync(
                    It.IsAny<CustomerCreateOptions>(),
                    It.IsAny<RequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnedCustomer);

            _mockCustomerService
                .Setup(x => x.Get(
                    user.PaymentProviderCustomerId,
                    It.IsAny<CustomerGetOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(returnedCustomer);

            _mockCustomerService
                .Setup(x => x.Update(
                    It.IsAny<string>(),
                    It.IsAny<CustomerUpdateOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new Customer
                {
                    Id = user.PaymentProviderCustomerId,
                    Balance = 100,
                    Address = new Address
                    {
                        Line1 = "21 BoolProp Lane",
                    }
                });

            _mockCustomerService
                .Setup(x => x.Delete(
                    It.IsAny<string>(),
                    It.IsAny<CustomerDeleteOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new Customer { Deleted = true });

            // Database setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            NectarineDbContext mockContext = new (options);

            _userCustomerService = new UserCustomerService(mockContext)
            {
                CustomerService = _mockCustomerService.Object,
            };
        }

        #region Customers

        [Fact(DisplayName = "AddStripeCustomerIdAsync should save a StripeId to the User")]
        public async Task Test_AddStripeCustomerIdAsync()
        {
            // Act
            await _userCustomerService.AddCustomerIdAsync(user);

            // Assert
            Assert.False(string.IsNullOrEmpty(user.PaymentProviderCustomerId));
        }
        
        #endregion

        #region GetCustomer

        [Fact(DisplayName = "GetCustomer should fetch a UserCustomerDetails object.")]
        public void Test_GetCustomer()
        {
            // Act
            var result = _userCustomerService.GetCustomer(user.PaymentProviderCustomerId);

            // Arrange
            Assert.IsType<UserCustomerDetails>(result);
        }

        [Fact(DisplayName = "GetCustomer should return null when a users customer information is not found.")]
        public void Test_GetCustomer_ReturnsNull()
        {
            // Act
            var result = _userCustomerService.GetCustomer("DoesntExist");

            // Arrange
            Assert.Null(result);
        }
        
        #endregion
        
        #region DeleteCustomer

        [Fact(DisplayName = "DeleteCustomer should delete the users Customer object.")]
        public void Test_DeleteCustomer()
        {
            // Act
            var result = _userCustomerService.DeleteCustomer(user);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "DeleteCustomer should return false when unable to delete a customer.")]
        public void Test_DeleteCustomer_FailsWhen_DeletingAUserWithoutACustomerObject()
        {
            // Arrange
            _mockCustomerService
                .Setup(x => x.Delete(
                    It.IsAny<string>(),
                    It.IsAny<CustomerDeleteOptions>(),
                    It.IsAny<RequestOptions>()));

            // Act
            var result = _userCustomerService.DeleteCustomer(user);

            // Assert
            Assert.False(result);
        }
        
        [Fact(DisplayName = "DeleteCustomer should throw an exception.")]
        public void Test_DeleteCustomer_ThrowsException()
        {
            // Arrange
            _mockCustomerService
                .Setup(x => x.Delete(
                    It.IsAny<string>(),
                    It.IsAny<CustomerDeleteOptions>(),
                    It.IsAny<RequestOptions>()))
                .Throws<StripeException>();

            // Act
            var result = _userCustomerService.DeleteCustomer(user);

            // Assert
            Assert.False(result);
        }
        
        #endregion

        #region UpdateCustomerAddress

        [Fact(DisplayName = "UpdateCustomerAddress should update a user address.")]
        public void Test_UpdateCustomerAddress()
        {
            // Act
            var result = _userCustomerService.UpdateCustomerAddress(
                user.PaymentProviderCustomerId,
                new UserAddress
                (
                    "21 BoolProp Lane",
                    null,
                    "Big City",
                    "11111",
                    "UK",
                    true
                ));

            // Arrange
            Assert.IsType<UserCustomerDetails>(result);
        }

        [Fact(DisplayName = "UpdateCustomerAddress should return null")]
        public void Test_UpdateCustomerAddress_ReturnsNull()
        {
            // Arrange
            _mockCustomerService
                .Setup(x => x.Update(
                    It.IsAny<string>(),
                    It.IsAny<CustomerUpdateOptions>(),
                    It.IsAny<RequestOptions>()));
            
            // Act
            var result = _userCustomerService.UpdateCustomerAddress(
                user.PaymentProviderCustomerId,
                new UserAddress
                (
                    "21 BoolProp Lane",
                    null,
                    "Big City",
                    "11111",
                    "UK",
                    true
                ));

            // Arrange
            Assert.Null(result);
        }

        #endregion
        
        #region UpdateCustomerPhoneNumber

        [Fact(DisplayName = "UpdateCustomerPhoneNumber should update a user address.")]
        public void Test_UpdateCustomerPhoneNumber()
        {
            // Act
            var result = _userCustomerService.UpdateCustomerPhoneNumber(
                user.PaymentProviderCustomerId,
                "07123123123");

            // Arrange
            Assert.IsType<UserCustomerDetails>(result);
        }

        [Fact(DisplayName = "UpdateCustomerPhoneNumber should return null")]
        public void Test_UpdateCustomerPhoneNumber_ReturnsNull()
        {
            // Arrange
            _mockCustomerService
                .Setup(x => x.Update(
                    It.IsAny<string>(),
                    It.IsAny<CustomerUpdateOptions>(),
                    It.IsAny<RequestOptions>()));
            
            // Act
            var result = _userCustomerService.UpdateCustomerPhoneNumber(
                user.PaymentProviderCustomerId,
                "07123123123");

            // Arrange
            Assert.Null(result);
        }

        #endregion
    }
}

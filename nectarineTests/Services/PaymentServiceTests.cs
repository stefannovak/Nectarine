using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NectarineAPI.Models.Payments;
using NectarineAPI.Services;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Services
{
    public class PaymentServiceTests
    {
        private readonly PaymentService _subject;
        private readonly Mock<PaymentMethodService> _paymentMethodServiceMock;
        private readonly Mock<PaymentIntentService> _paymentIntentServiceMock;
        private readonly ApplicationUser user = new () { Id = Guid.NewGuid() };
        private readonly PaymentMethod fakePaymentMethod = new ()
        {
            Id = "FakePaymentMethodId",
            Card = new PaymentMethodCard
            {
                ExpMonth = 12,
                ExpYear = 2025,
                Last4 = "4242",
            },
        };

        public PaymentServiceTests()
        {
            // PaymentMethodServiceMock setup
            _paymentMethodServiceMock = new Mock<PaymentMethodService>();

            _paymentMethodServiceMock
                .Setup(x => x.Create(
                    It.IsAny<PaymentMethodCreateOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(fakePaymentMethod);

            _paymentMethodServiceMock
                .Setup(x => x.Attach(
                    It.IsAny<string>(),
                    It.IsAny<PaymentMethodAttachOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(fakePaymentMethod);

            _paymentMethodServiceMock
                .Setup(x => x.Get(
                    It.IsAny<string>(),
                    It.IsAny<PaymentMethodGetOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(fakePaymentMethod);

            _paymentMethodServiceMock
                .Setup(x => x.List(
                    It.IsAny<PaymentMethodListOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new StripeList<PaymentMethod>
                {
                    Data = new List<PaymentMethod>
                    {
                        fakePaymentMethod,
                        fakePaymentMethod,
                    },
                });

            // PaymentIntentServiceMock setup
            _paymentIntentServiceMock = new Mock<PaymentIntentService>();

            _paymentIntentServiceMock
                .Setup(x => x.Create(
                    It.IsAny<PaymentIntentCreateOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new PaymentIntent
                {
                    ClientSecret = "ClientSecret",
                });

            _paymentIntentServiceMock
                .Setup(x => x.Confirm(
                    It.IsAny<string>(),
                    It.IsAny<PaymentIntentConfirmOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new PaymentIntent
                {
                    Status = "succeeded",
                });

            // PaymentService setup
            _subject = new PaymentService()
            {
                PaymentIntentService = _paymentIntentServiceMock.Object,
                PaymentMethodService = _paymentMethodServiceMock.Object,
            };
        }

        [Fact(DisplayName = "AddCardPaymentMethod should add a reference for a card to the user.")]
        public void Test_AddCardPaymentMethod()
        {
            // Act
            var result = _subject.AddCardPaymentMethod(
                user.PaymentProviderCustomerId,
                "4242424242424242",
                9,
                2025,
                "552");

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "AddCardToPaymentMethod should return false when fails to create a payment method")]
        public void Test_AddCardPaymentMethod_ShouldFailWithInvalidCardDetails()
        {
            // Arrange
            _paymentMethodServiceMock
                .Setup(x => x.Create(
                    It.IsAny<PaymentMethodCreateOptions>(),
                    It.IsAny<RequestOptions>()))
                .Throws(new StripeException());

            // Act
            var result = _subject.AddCardPaymentMethod(
                user.PaymentProviderCustomerId,
                "4242424242424242",
                9,
                2025,
                "0");

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "GetCardsForUser should return a list of cards attached to the user")]
        public void Test_GetCardsForUser()
        {
            // Act
            var cards = _subject.GetCardsForUser(user.PaymentProviderCustomerId);

            // Assert
            Assert.True(cards.Any());
        }

        [Fact(DisplayName = "GetPaymentMethod should return a payment method")]
        public void Test_GetPaymentMethod()
        {
            // Act
            var result = _subject.GetPaymentMethod("paymentMethodId");

            // Assert
            Assert.IsType<SensitivePaymentMethod>(result);
        }

        #region CreatePaymentIntent

        [Fact(DisplayName = "CreatePaymentIntent should create a PaymentIntent and return a CreatePaymentIntentResponse")]
        public void Test_CreatePaymentIntent()
        {
            // Act
            var result = _subject.CreatePaymentIntent(user.PaymentProviderCustomerId, 500, "paymentMethodId");

            // Assert
            Assert.IsType<PaymentIntentResponse>(result);
        }
        
        [Fact(DisplayName = "CreatePaymentIntent should fail to create a PaymentIntent and return null")]
        public void Test_CreatePaymentIntent_ReturnsNull()
        {
            // Arrange
            _paymentIntentServiceMock
                .Setup(x => x.Create(
                    It.IsAny<PaymentIntentCreateOptions>(),
                    It.IsAny<RequestOptions>()));
            
            // Act
            var result = _subject.CreatePaymentIntent(
                user.PaymentProviderCustomerId,
                500,
                "paymentMethodId");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ConfirmPaymentIntent

        [Fact(DisplayName = "ConfirmPaymentIntent should confirm a PaymentIntent with a given client secret")]
        public void Test_ConfirmPaymentIntent()
        {
            // Act
            var result = _subject.ConfirmPaymentIntent("paymentMethodId");

            // Assert
            Assert.IsType<PaymentIntentResponse>(result);
        }
        
        [Fact(DisplayName = "ConfirmPaymentIntent should fail to confirm a PaymentIntent and return null")]
        public void Test_ConfirmPaymentIntent_ReturnsNull()
        {
            // Arrange
            _paymentIntentServiceMock
                .Setup(x => x.Confirm(
                    It.IsAny<string>(),
                    It.IsAny<PaymentIntentConfirmOptions>(),
                    It.IsAny<RequestOptions>()));
            
            // Act
            var result = _subject.ConfirmPaymentIntent("paymentMethodId");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}

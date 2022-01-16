using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Moq;
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
        private readonly ApplicationUser user = new () { Id = Guid.NewGuid().ToString() };
        private readonly PaymentMethod fakePaymentMethod = new () { Id = "FakePaymentMethodId" };

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
            var paymentIntentServiceMock = new Mock<PaymentIntentService>();

            paymentIntentServiceMock
                .Setup(x => x.Create(
                    It.IsAny<PaymentIntentCreateOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new PaymentIntent
                {
                    ClientSecret = "ClientSecret",
                });

            paymentIntentServiceMock
                .Setup(x => x.Confirm(
                    It.IsAny<string>(),
                    It.IsAny<PaymentIntentConfirmOptions>(),
                    It.IsAny<RequestOptions>()))
                .Returns(new PaymentIntent
                {
                    Status = "succeeded",
                });

            // Logger setup
            var loggerMock = new Mock<ILogger<PaymentService>>();

            // PaymentService setup
            _subject = new PaymentService(loggerMock.Object)
            {
                PaymentIntentService = paymentIntentServiceMock.Object,
                PaymentMethodService = _paymentMethodServiceMock.Object,
            };
        }

        [Fact(DisplayName = "AddCardPaymentMethod should add a reference for a card to the user.")]
        public void Test_AddCardPaymentMethod()
        {
            // Act
            var result = _subject.AddCardPaymentMethod(
                user,
                "4242424242424242",
                9,
                2025,
                "552");

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "AddCardToPaymentMethod should throw an exception Stripe fails to create a payment method")]
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
                user,
                "4242424242424242",
                9,
                2025,
                "0");

            // Assert
            Assert.IsType<StripeException>(result);
        }

        [Fact(DisplayName = "GetCardsForUser should return a list of cards attached to the user")]
        public void Test_GetCardsForUser()
        {
            // Act
            var cards = _subject.GetCardsForUser(user);

            // Assert
            Assert.True((cards as StripeList<PaymentMethod>)?.Data.Any() == true);
        }

        [Fact(DisplayName = "CreatePaymentIntent should create a PaymentIntent and attach it to the user's Customer object")]
        public void Test_CreatePaymentIntent()
        {
            // Act
            var paymentIntent = _subject.CreatePaymentIntent(user, 500, "paymentMethodId");

            // Assert
            Assert.False(paymentIntent.ClientSecret.IsNullOrEmpty());
        }

        [Fact(DisplayName = "ConfirmPaymentIntent should confirm a PaymentIntent with a given client secret")]
        public void Test_ConfirmPaymentIntent()
        {
            // Act
            var result = _subject.ConfirmPaymentIntent("paymentMethodId");

            // Assert
            Assert.True(result.Status == "succeeded");
        }
    }
}

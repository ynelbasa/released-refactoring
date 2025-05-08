using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Interfaces;
using RefactorThis.Domain.Services;

namespace RefactorThis.Domain.Tests.Services
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly InvoicePaymentProcessor _invoicePaymentProcessor;

        public InvoicePaymentProcessorTests()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _invoicePaymentProcessor = new InvoicePaymentProcessor(_invoiceRepositoryMock.Object);
        }

        [Test]
        public void Should_ReturnFailureMessage_When_NoInvoiceFoundForPaymentReference()
        {
            // Arrange
            Invoice nullInvoice = null;
            var payment = new Payment();
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(nullInvoice);
            const string failureMessage = "There is no invoice matching this payment";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(failureMessage, result.GetMessage());
        }

        [Test]
        public void Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };
            var payment = new Payment();
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "no payment needed";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(failureMessage, result.GetMessage());
        }

        [Test]
        public void Should_ReturnFailureMessage_When_InvoiceIsInvalidHasZeroAmountAndHasPayments()
        {
            // Arrange
            var payment = new Payment { Amount = 20 };
            var invoice = new Invoice { Amount = 0, Payments = new List<Payment> { payment } };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);

            const string failureMessage =
                "The invoice is in an invalid state, it has an amount of 0 and it has payments.";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(failureMessage, result.GetMessage());
        }

        [Test]
        public void Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 10 }
                }
            };
            var payment = new Payment();
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "invoice was already fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(failureMessage, result.GetMessage());
        }

        [Test]
        public void Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };
            var payment = new Payment { Amount = 6 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "the payment is greater than the partial amount remaining";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(failureMessage, result.GetMessage());
        }

        [Test]
        public void Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            var payment = new Payment { Amount = 6 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "the payment is greater than the invoice amount";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(failureMessage, result.GetMessage());
        }

        [TestCase(InvoiceType.Standard)]
        [TestCase(InvoiceType.Commercial)]
        public void Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue(
            InvoiceType type)
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                },
                Type = type
            };
            var payment = new Payment { Amount = 5 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string successMessage = "final partial payment received, invoice is now fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.AreEqual(successMessage, result.GetMessage());
        }

        [TestCase(InvoiceType.Standard)]
        [TestCase(InvoiceType.Commercial)]
        public void
            Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount(InvoiceType type)
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            var payment = new Payment { Amount = 10 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string successMessage = "invoice was already fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.AreEqual(successMessage, result.GetMessage());
        }

        [TestCase(InvoiceType.Standard)]
        [TestCase(InvoiceType.Commercial)]
        public void
            Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue(
                InvoiceType type)
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                },
                Type = type
            };
            var payment = new Payment { Amount = 1 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string successMessage = "another partial payment received, still not fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.AreEqual(successMessage, result.GetMessage());
        }

        [TestCase(InvoiceType.Standard)]
        [TestCase(InvoiceType.Commercial)]
        public void Should_ReturnFullyPaidMessage_When_AmountPaidIsEqualToInvoiceAmount(InvoiceType type)
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                Type = type,
                Payments = new List<Payment>()
            };
            var payment = new Payment { Amount = 10 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string successMessage = "invoice is now fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.AreEqual(successMessage, result.GetMessage());
        }

        [TestCase(InvoiceType.Standard)]
        [TestCase(InvoiceType.Commercial)]
        public void Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount(
            InvoiceType type)
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = type
            };
            var payment = new Payment { Amount = 1 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string successMessage = "invoice is now partially paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.AreEqual(successMessage, result.GetMessage());
        }
    }
}
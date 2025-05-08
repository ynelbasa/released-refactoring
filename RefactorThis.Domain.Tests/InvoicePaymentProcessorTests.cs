using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RefactorThis.Domain.Interfaces;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
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
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            // Arrange
            Invoice nullInvoice = null;
            var payment = new Payment();
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(nullInvoice);
            const string failureMessage = "There is no invoice matching this payment";

            // Act
            // Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _invoicePaymentProcessor.ProcessPayment(payment));
            Assert.AreEqual(failureMessage, exception.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
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
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_InvoiceIsInvalidHasZeroAmountAndHasPayments()
        {
            // Arrange
            var payment = new Payment { Amount = 20 };
            var invoice = new Invoice { Amount = 0, Payments = new List<Payment> { payment } };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);

            const string failureMessage =
                "The invoice is in an invalid state, it has an amount of 0 and it has payments.";

            // Act
            // Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _invoicePaymentProcessor.ProcessPayment(payment));
            Assert.AreEqual(failureMessage, exception.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
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
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
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
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
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
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPaymentForStandardInvoice_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
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
                Type = InvoiceType.Standard
            };
            var payment = new Payment { Amount = 5 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "final partial payment received, invoice is now fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPaymentForCommercialInvoice_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
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
                Type = InvoiceType.Commercial
            };
            var payment = new Payment { Amount = 5 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "final partial payment received, invoice is now fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
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
            const string failureMessage = "invoice was already fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
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
                Type = InvoiceType.Standard
            };
            var payment = new Payment { Amount = 1 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "another partial payment received, still not fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPaymentForCommercialInvoice_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
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
                Type = InvoiceType.Commercial
            };
            var payment = new Payment { Amount = 1 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void
            ProcessPaymentForStandardInvoice_Should_ReturnFullyPaidMessage_When_AmountPaidIsEqualToInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                Type = InvoiceType.Standard,
                Payments = new List<Payment>()
            };
            var payment = new Payment { Amount = 10 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "invoice is now fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPaymentForCommercialInvoice_Should_ReturnFullyPaidMessage_When_AmountPaidIsEqualToInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                Type = InvoiceType.Commercial,
                Payments = new List<Payment>()
            };
            var payment = new Payment { Amount = 10 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "invoice is now fully paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPaymentForStandardInvoice_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Standard
            };
            var payment = new Payment { Amount = 1 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "invoice is now partially paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }

        [Test]
        public void
            ProcessPaymentForCommercialInvoice_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Commercial
            };
            var payment = new Payment { Amount = 1 };
            _invoiceRepositoryMock.Setup(x => x.GetInvoice(payment.Reference)).Returns(invoice);
            const string failureMessage = "invoice is now partially paid";

            // Act
            var result = _invoicePaymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(failureMessage, result);
        }
    }
}
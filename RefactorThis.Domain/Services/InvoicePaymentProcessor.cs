using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Exceptions;
using RefactorThis.Domain.Interfaces;

namespace RefactorThis.Domain.Services
{
    public class InvoicePaymentProcessor
    {
        private readonly IInvoiceRepository _invoiceRepository;

        private readonly Dictionary<InvoiceType, Action<Payment, Invoice>> _succeedingPaymentCalculators =
            new Dictionary<InvoiceType, Action<Payment, Invoice>>
            {
                { InvoiceType.Standard, CalculateStandardInvoiceSucceedingPayment },
                { InvoiceType.Commercial, CalculateCommercialInvoiceSucceedingPayment }
            };

        private readonly Dictionary<InvoiceType, Action<Payment, Invoice>> _initialPaymentCalculators =
            new Dictionary<InvoiceType, Action<Payment, Invoice>>
            {
                { InvoiceType.Standard, CalculateInitialPayment },
                { InvoiceType.Commercial, CalculateInitialPayment }
            };

        public InvoicePaymentProcessor(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);
            ValidateInvoice(invoice);

            var isPaymentNotRequired = invoice.Amount == 0 && (invoice.Payments == null || !invoice.Payments.Any());
            if (isPaymentNotRequired) return "no payment needed";

            var isInvoiceFullyPaid = invoice.Payments.Sum(x => x.Amount) != 0 &&
                                     invoice.Amount == invoice.Payments.Sum(x => x.Amount);
            if (isInvoiceFullyPaid) return "invoice was already fully paid";

            var isOverpayingPartialAmount = invoice.Payments.Sum(x => x.Amount) != 0 &&
                                            payment.Amount > (invoice.Amount - invoice.AmountPaid);
            if (isOverpayingPartialAmount)
                return "the payment is greater than the partial amount remaining";

            var isOverpayingInvoiceAmount = payment.Amount > invoice.Amount;
            if (isOverpayingInvoiceAmount)
                return "the payment is greater than the invoice amount";

            var hasInitialPayment = invoice.Payments != null && invoice.Payments.Any();
            var responseMessage = hasInitialPayment
                ? ProcessSucceedingPayment(payment, invoice)
                : ProcessInitialPayment(payment, invoice);

            _invoiceRepository.SaveInvoice(invoice);
            return responseMessage;
        }

        private string ProcessSucceedingPayment(Payment payment, Invoice invoice)
        {
            var isFinalPayment = (invoice.Amount - invoice.AmountPaid) == payment.Amount;
            _succeedingPaymentCalculators[invoice.Type](payment, invoice);
            var responseMessage = isFinalPayment
                ? "final partial payment received, invoice is now fully paid"
                : "another partial payment received, still not fully paid";
            return responseMessage;
        }

        private string ProcessInitialPayment(Payment payment, Invoice invoice)
        {
            var isFullPayment = invoice.Amount == payment.Amount;
            _initialPaymentCalculators[invoice.Type](payment, invoice);
            var responseMessage = isFullPayment ? "invoice is now fully paid" : "invoice is now partially paid";
            return responseMessage;
        }

        private static void ValidateInvoice(Invoice invoice)
        {
            if (invoice == null)
                throw new MissingInvoiceException();

            if (invoice.Amount == 0 && invoice.Payments != null)
                throw new InvalidInvoiceStateException();
        }

        private static void CalculateInitialPayment(Payment payment, Invoice invoice)
        {
            invoice.AmountPaid = payment.Amount;
            invoice.TaxAmount = payment.Amount * 0.14m;
            invoice.Payments.Add(payment);
        }

        private static void CalculateStandardInvoiceSucceedingPayment(Payment payment, Invoice invoice)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);
        }

        private static void CalculateCommercialInvoiceSucceedingPayment(Payment payment, Invoice invoice)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount += payment.Amount * 0.14m;
            invoice.Payments.Add(payment);
        }
    }
}
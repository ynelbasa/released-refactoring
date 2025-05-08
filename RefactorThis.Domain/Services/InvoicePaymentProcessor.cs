using System;
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

        private static string ProcessSucceedingPayment(Payment payment, Invoice invoice)
        {
            string responseMessage;
            if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
            {
                switch (invoice.Type)
                {
                    case InvoiceType.Standard:
                        invoice.AmountPaid += payment.Amount;
                        invoice.Payments.Add(payment);
                        responseMessage = "final partial payment received, invoice is now fully paid";
                        break;
                    case InvoiceType.Commercial:
                        invoice.AmountPaid += payment.Amount;
                        invoice.TaxAmount += payment.Amount * 0.14m;
                        invoice.Payments.Add(payment);
                        responseMessage = "final partial payment received, invoice is now fully paid";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (invoice.Type)
                {
                    case InvoiceType.Standard:
                        invoice.AmountPaid += payment.Amount;
                        invoice.Payments.Add(payment);
                        responseMessage = "another partial payment received, still not fully paid";
                        break;
                    case InvoiceType.Commercial:
                        invoice.AmountPaid += payment.Amount;
                        invoice.TaxAmount += payment.Amount * 0.14m;
                        invoice.Payments.Add(payment);
                        responseMessage = "another partial payment received, still not fully paid";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return responseMessage;
        }

        private static string ProcessInitialPayment(Payment payment, Invoice invoice)
        {
            string responseMessage;
            if (invoice.Amount == payment.Amount)
            {
                switch (invoice.Type)
                {
                    case InvoiceType.Standard:
                    case InvoiceType.Commercial:
                        invoice.AmountPaid = payment.Amount;
                        invoice.TaxAmount = payment.Amount * 0.14m;
                        invoice.Payments.Add(payment);
                        responseMessage = "invoice is now fully paid";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                switch (invoice.Type)
                {
                    case InvoiceType.Standard:
                    case InvoiceType.Commercial:
                        invoice.AmountPaid = payment.Amount;
                        invoice.TaxAmount = payment.Amount * 0.14m;
                        invoice.Payments.Add(payment);
                        responseMessage = "invoice is now partially paid";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return responseMessage;
        }

        private static void ValidateInvoice(Invoice invoice)
        {
            if (invoice == null)
                throw new MissingInvoiceException();

            if (invoice.Amount == 0 && invoice.Payments != null)
                throw new InvalidInvoiceStateException();
        }
    }
}
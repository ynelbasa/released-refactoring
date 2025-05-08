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

            var responseMessage = string.Empty;

            ValidateInvoice(invoice);

            var isFullyPaid = invoice.Amount == 0 && (invoice.Payments == null || !invoice.Payments.Any());
            if (isFullyPaid)
            {
                responseMessage = "no payment needed";
                return responseMessage;
            }

            if (invoice.Payments != null && invoice.Payments.Any())
            {
                if (invoice.Payments.Sum(x => x.Amount) != 0 &&
                    invoice.Amount == invoice.Payments.Sum(x => x.Amount))
                {
                    responseMessage = "invoice was already fully paid";
                }
                else if (invoice.Payments.Sum(x => x.Amount) != 0 &&
                         payment.Amount > (invoice.Amount - invoice.AmountPaid))
                {
                    responseMessage = "the payment is greater than the partial amount remaining";
                }
                else
                {
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
                }
            }
            else
            {
                if (payment.Amount > invoice.Amount)
                {
                    responseMessage = "the payment is greater than the invoice amount";
                }
                else if (invoice.Amount == payment.Amount)
                {
                    switch (invoice.Type)
                    {
                        case InvoiceType.Standard:
                            invoice.AmountPaid = payment.Amount;
                            invoice.TaxAmount = payment.Amount * 0.14m;
                            invoice.Payments.Add(payment);
                            responseMessage = "invoice is now fully paid";
                            break;
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
                            invoice.AmountPaid = payment.Amount;
                            invoice.TaxAmount = payment.Amount * 0.14m;
                            invoice.Payments.Add(payment);
                            responseMessage = "invoice is now partially paid";
                            break;
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
            }

            _invoiceRepository.SaveInvoice(invoice);
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
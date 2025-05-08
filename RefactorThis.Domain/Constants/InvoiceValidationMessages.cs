namespace RefactorThis.Domain.Constants
{
    public static class InvoiceValidationMessages
    {
        public const string PaymentExceedsInvoiceAmount = "the payment is greater than the invoice amount";
        public const string PaymentExceedsPartialAmount = "the payment is greater than the partial amount remaining";
        public const string InvoiceAlreadyFullyPaid = "invoice was already fully paid";
        public const string PaymentNotRequired = "no payment needed";
    }
}
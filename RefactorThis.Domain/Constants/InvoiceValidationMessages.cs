namespace RefactorThis.Domain.Constants
{
    public static class InvoiceValidationMessages
    {
        public const string InvoiceNotFound = "There is no invoice matching this payment";
        public const string InvalidInvoice = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
        public const string InvalidInvoiceType = "Unable to calculate payment due to unsupported invoice type";
        public const string PaymentExceedsInvoiceAmount = "the payment is greater than the invoice amount";
        public const string PaymentExceedsPartialAmount = "the payment is greater than the partial amount remaining";
        public const string InvoiceAlreadyFullyPaid = "invoice was already fully paid";
        public const string PaymentNotRequired = "no payment needed";
    }
}
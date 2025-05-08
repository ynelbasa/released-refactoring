namespace RefactorThis.Domain.Constants
{
    public static class InvoiceMessages
    {
        public const string FullyPaid = "Invoice is now fully paid";
        public const string PartiallyPaid = "Invoice is now partially paid";
        public const string FinalPaymentReceived = "Final partial payment received, invoice is now fully paid";
        public const string PartialPaymentReceived = "Another partial payment received, still not fully paid";
    }
}
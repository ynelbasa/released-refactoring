namespace RefactorThis.Domain.Constants
{
    public static class InvoiceMessages
    {
        public const string FullyPaid = "invoice is now fully paid";
        public const string PartiallyPaid = "invoice is now partially paid";
        public const string FinalPaymentReceived = "final partial payment received, invoice is now fully paid";
        public const string PartialPaymentReceived = "another partial payment received, still not fully paid";
    }
}
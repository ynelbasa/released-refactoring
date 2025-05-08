using System;

namespace RefactorThis.Domain.Exceptions
{
    public class InvalidInvoiceStateException : InvalidOperationException
    {
        public InvalidInvoiceStateException()
            : base("The invoice is in an invalid state, it has an amount of 0 and it has payments.")
        {
        }
    }
}
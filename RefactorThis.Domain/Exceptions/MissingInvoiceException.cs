using System;

namespace RefactorThis.Domain.Exceptions
{
    public class MissingInvoiceException : InvalidOperationException
    {
        public MissingInvoiceException() : base("There is no invoice matching this payment")
        {
        }
    }
}
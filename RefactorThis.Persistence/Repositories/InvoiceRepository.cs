using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Interfaces;

namespace RefactorThis.Persistence.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string paymentReference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }
    }
}
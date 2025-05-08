using RefactorThis.Business.Interfaces;
using RefactorThis.Domain.Entities;

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
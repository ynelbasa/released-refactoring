using RefactorThis.Persistence;

namespace RefactorThis.Domain.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string paymentReference);
        void SaveInvoice(Invoice invoice);
    }
}
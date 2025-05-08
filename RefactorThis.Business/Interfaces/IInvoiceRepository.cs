using RefactorThis.Domain.Entities;

namespace RefactorThis.Business.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string paymentReference);
        void SaveInvoice(Invoice invoice);
    }
}
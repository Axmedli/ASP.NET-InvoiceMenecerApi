using InvoiceMenecerApi.DTOs.InvoiceDto;

namespace InvoiceMenecerApi.Services.Interfaces;

public interface IInvoiceRowService
{
    Task<IEnumerable<InvoiceRowDto>> GetRowsByInvoiceIdAsync(Guid invoiceId);
    Task<InvoiceRowDto?> GetRowByIdAsync(Guid rowId);
    Task<InvoiceRowDto> AddRowToInvoiceAsync(Guid invoiceId, CreateInvoiceRowDto createRowDto);
    Task<InvoiceRowDto> UpdateRowAsync(Guid rowId, CreateInvoiceRowDto updateRowDto);
    Task<bool> DeleteRowAsync(Guid rowId);
}
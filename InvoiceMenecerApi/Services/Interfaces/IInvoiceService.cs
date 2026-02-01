using InvoiceMenecer.Models;
using InvoiceMenecerApi.DTOs.InvoiceDto;

namespace InvoiceMenecerApi.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto);
    Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceDto updateInvoiceDto);
    Task<InvoiceResponseDto> ChangeInvoiceStatusAsync(Guid invoiceId, InvoiceStatus newStatus);
    Task<bool> HardDeleteInvoiceAsync(Guid invoiceId);
    Task<bool> ArchiveInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync();
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId);
    Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByCustomerIdAsync(Guid customerId);
}
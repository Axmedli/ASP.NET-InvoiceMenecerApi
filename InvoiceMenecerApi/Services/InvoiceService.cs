using AutoMapper;
using InvoiceMenecer.Models;
using InvoiceMenecerApi.DTOs.InvoiceDto;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceMenecerApi.Services;

public class InvoiceService : IInvoiceService
{
    private readonly InvoiceMenecerDBContext _context;
    private readonly IMapper _mapper;

    public InvoiceService(InvoiceMenecerDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public async Task<InvoiceResponseDto> ChangeInvoiceStatusAsync(Guid invoiceId, InvoiceStatus newStatus)
    {
        var invoice = await _context
            .Invoices
            .Include(i => i.Rows)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (invoice == null) return null;

        invoice.Status = newStatus;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<InvoiceResponseDto>(invoice);
    }

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto)
    {
        var invoice = _mapper.Map<Invoice>(createInvoiceDto);

        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum);

        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync();
        await _context
            .Entry(invoice)
            .Reference(i => i.Customer)
            .LoadAsync();

        return _mapper.Map<InvoiceResponseDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync()
    {
        var invoices = await _context
            .Invoices
            .Include(i => i.Customer)
            .Include(i => i.Rows)
            .Where(i => i.DeletedAt == null)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        var invoice = await _context
            .Invoices
            .Include(i => i.Customer)
            .Include(i => i.Rows)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (invoice == null)
            return null;

        return _mapper.Map<InvoiceResponseDto?>(invoice);
    }

    public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByCustomerIdAsync(Guid customerId)
    {
        var invoices = await _context
            .Invoices
            .Include(i => i.Customer)
            .Include(i => i.Rows)
            .Where(i => i.CustomerId == customerId && i.DeletedAt == null)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
    }

    public async Task<bool> HardDeleteInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _context
            .Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (invoice == null) return false;

        if(invoice.Status != InvoiceStatus.Created) return false;

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> ArchiveInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _context
            .Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (invoice == null) return false;

        invoice.DeletedAt = DateTimeOffset.UtcNow;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<InvoiceResponseDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceDto updateInvoiceDto)
    {
        var updatedInvoice = await _context
                                 .Invoices
                                 .Include(i => i.Rows)
                                 .Include(i => i.Customer)
                                 .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (updatedInvoice == null) return null;

        if (updatedInvoice.Status != InvoiceStatus.Created) return null;

        _context.InvoiceRows.RemoveRange(updatedInvoice.Rows);

        _mapper.Map(updateInvoiceDto, updatedInvoice);

        updatedInvoice.TotalSum = updatedInvoice.Rows.Sum(r => r.Sum);
        updatedInvoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<InvoiceResponseDto>(updatedInvoice);
    }
}

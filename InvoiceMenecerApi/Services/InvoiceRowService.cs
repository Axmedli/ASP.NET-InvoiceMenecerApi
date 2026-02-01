using AutoMapper;
using InvoiceMenecer.Models;
using InvoiceMenecerApi.DTOs.InvoiceDto;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceMenecerApi.Services;

public class InvoiceRowService : IInvoiceRowService
{
    private readonly InvoiceMenecerDBContext _context;
    private readonly IMapper _mapper;

    public InvoiceRowService(InvoiceMenecerDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InvoiceRowDto>> GetRowsByInvoiceIdAsync(Guid invoiceId)
    {
        var rows = await _context.InvoiceRows
            .Where(r => r.InvoiceId == invoiceId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InvoiceRowDto>>(rows);
    }

    public async Task<InvoiceRowDto?> GetRowByIdAsync(Guid rowId)
    {
        var row = await _context.InvoiceRows
            .FirstOrDefaultAsync(r => r.Id == rowId);

        if (row == null)
            return null;

        return _mapper.Map<InvoiceRowDto>(row);
    }

    public async Task<InvoiceRowDto> AddRowToInvoiceAsync(Guid invoiceId, CreateInvoiceRowDto createRowDto)
    {
        // Invoice-u tap və yoxla
        var invoice = await _context.Invoices
            .Include(i => i.Rows)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (invoice == null)
            return null;

        // Yalnız göndərilməmiş invoice-lara row əlavə et
        if (invoice.Status != InvoiceStatus.Created)
            return null;

        // Yeni row yarat
        var row = _mapper.Map<InvoiceRow>(createRowDto);
        row.Id = Guid.NewGuid();
        row.InvoiceId = invoiceId;

        _context.InvoiceRows.Add(row);

        // Invoice-un TotalSum-unu yenilə
        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum) + row.Sum;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<InvoiceRowDto>(row);
    }

    public async Task<InvoiceRowDto> UpdateRowAsync(Guid rowId, CreateInvoiceRowDto updateRowDto)
    {
        var row = await _context.InvoiceRows
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.Id == rowId);

        if (row == null)
            return null;

        // Yalnız göndərilməmiş invoice-ların row-larını yenilə
        if (row.Invoice.Status != InvoiceStatus.Created || row.Invoice.DeletedAt != null)
            return null;

        var oldSum = row.Sum;

        // Row-u yenilə
        _mapper.Map(updateRowDto, row);

        // Invoice-un TotalSum-unu yenilə
        row.Invoice.TotalSum = row.Invoice.TotalSum - oldSum + row.Sum;
        row.Invoice.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<InvoiceRowDto>(row);
    }

    public async Task<bool> DeleteRowAsync(Guid rowId)
    {
        var row = await _context.InvoiceRows
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.Id == rowId);

        if (row == null)
            return false;

        // Yalnız göndərilməmiş invoice-ların row-larını sil
        if (row.Invoice.Status != InvoiceStatus.Created || row.Invoice.DeletedAt != null)
            return false;

        // Invoice-un TotalSum-unu yenilə
        row.Invoice.TotalSum -= row.Sum;
        row.Invoice.UpdatedAt = DateTimeOffset.UtcNow;

        _context.InvoiceRows.Remove(row);
        await _context.SaveChangesAsync();

        return true;
    }
}
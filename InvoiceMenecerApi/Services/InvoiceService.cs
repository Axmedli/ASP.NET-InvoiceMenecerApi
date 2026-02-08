using ASP_NET_08.Common;
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

    public async Task<PagedResult<InvoiceResponseDto>> GetAllInvoicesPagedAsync(InvoiceQueryParams queryParams)
    {
        queryParams.Validate();

        var query = _context
            .Invoices
            .Include(i => i.Customer)
            .Include(i => i.Rows)
            .Where(i => i.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchTerm = queryParams.Search.ToLower();
            query = query.Where(i =>
                (i.Comment != null && i.Comment.ToLower().Contains(searchTerm)) ||
                i.Customer.Name.ToLower().Contains(searchTerm)
            );
        }

        if (queryParams.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == queryParams.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            if (Enum.TryParse<InvoiceStatus>(queryParams.Status, out var status))
            {
                query = query.Where(i => i.Status == status);
            }
        }

        if (queryParams.StartDateFrom.HasValue)
        {
            query = query.Where(i => i.StartDate >= queryParams.StartDateFrom.Value);
        }
        if (queryParams.StartDateTo.HasValue)
        {
            query = query.Where(i => i.StartDate <= queryParams.StartDateTo.Value);
        }

        if (queryParams.EndDateFrom.HasValue)
        {
            query = query.Where(i => i.EndDate >= queryParams.EndDateFrom.Value);
        }
        if (queryParams.EndDateTo.HasValue)
        {
            query = query.Where(i => i.EndDate <= queryParams.EndDateTo.Value);
        }

        if (queryParams.MinTotalSum.HasValue)
        {
            query = query.Where(i => i.TotalSum >= queryParams.MinTotalSum.Value);
        }
        if (queryParams.MaxTotalSum.HasValue)
        {
            query = query.Where(i => i.TotalSum <= queryParams.MaxTotalSum.Value);
        }

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = ApplySorting(query, queryParams.Sort, queryParams.SortDirection);
        }
        else
        {
            query = query.OrderByDescending(i => i.CreatedAt);
        }

        var skip = (queryParams.Page - 1) * queryParams.Size;
        var invoices = await query
            .Skip(skip)
            .Take(queryParams.Size)
            .ToListAsync();

        var invoiceDtos = _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);

        return PagedResult<InvoiceResponseDto>.Create(
            items: invoiceDtos,
            page: queryParams.Page,
            pageSize: queryParams.Size,
            totalCount: totalCount
        );
    }

    private IQueryable<Invoice> ApplySorting(
        IQueryable<Invoice> query,
        string sort,
        string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sort.ToLower() switch
        {
            "customerid" => isDescending
                ? query.OrderByDescending(i => i.CustomerId)
                : query.OrderBy(i => i.CustomerId),
            "startdate" => isDescending
                ? query.OrderByDescending(i => i.StartDate)
                : query.OrderBy(i => i.StartDate),
            "enddate" => isDescending
                ? query.OrderByDescending(i => i.EndDate)
                : query.OrderBy(i => i.EndDate),
            "totalsum" => isDescending
                ? query.OrderByDescending(i => i.TotalSum)
                : query.OrderBy(i => i.TotalSum),
            "status" => isDescending
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            "createdat" => isDescending
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt),
            "updatedat" => isDescending
                ? query.OrderByDescending(i => i.UpdatedAt)
                : query.OrderBy(i => i.UpdatedAt),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };
    }
}

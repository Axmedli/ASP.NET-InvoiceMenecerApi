using AutoMapper;
using InvoiceMenecerApi.Common;
using InvoiceMenecerApi.DTOs.InvoiceDto;
using InvoiceMenecerApi.Models;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        if (invoice == null) return null!;

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

        if (updatedInvoice == null) return null!;

        if (updatedInvoice.Status != InvoiceStatus.Created) return null!;

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

    public async Task<byte[]> DownloadInvoiceAsPdfAsync(Guid invoiceId)
    {
        var invoice = await _context
            .Invoices
            .Include(i => i.Customer)
            .Include(i => i.Rows)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.DeletedAt == null);

        if (invoice == null)
            throw new Exception("Invoice not found");

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Column(col =>
                    {
                        col.Item().Text($"INVOICE #{invoice.Id.ToString().Substring(0, 8).ToUpper()}")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        col.Item().PaddingTop(5).BorderBottom(2).BorderColor(Colors.Grey.Medium);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("BILL TO:").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                                column.Item().PaddingTop(5).Text(invoice.Customer.Name).FontSize(12).Bold();
                                column.Item().Text(invoice.Customer.Email).FontSize(10);
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.PhoneNumber))
                                    column.Item().Text(invoice.Customer.PhoneNumber).FontSize(10);
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.Address))
                                    column.Item().Text(invoice.Customer.Address).FontSize(10);
                            });

                            row.RelativeItem().Column(column =>
                            {
                                column.Item().AlignRight().Text("INVOICE DETAILS").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                                column.Item().PaddingTop(5).AlignRight().Text($"Date: {invoice.CreatedAt:dd/MM/yyyy}").FontSize(10);
                                column.Item().AlignRight().Text($"Period: {invoice.StartDate:dd/MM/yyyy} - {invoice.EndDate:dd/MM/yyyy}").FontSize(10);
                                column.Item().AlignRight().Text($"Status: {invoice.Status}").FontSize(10).Bold()
                                    .FontColor(invoice.Status == InvoiceStatus.Paid ? Colors.Green.Medium : Colors.Orange.Medium);
                            });
                        });

                        col.Item().PaddingTop(30);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("SERVICE").Bold();
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("QUANTITY").Bold();
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("AMOUNT").Bold();
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("SUM").Bold();

                                static IContainer HeaderCellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(2)
                                        .BorderColor(Colors.Blue.Medium)
                                        .Background(Colors.Blue.Lighten5)
                                        .Padding(10);
                                }
                            });

                            foreach (var row in invoice.Rows)
                            {
                                table.Cell().Element(DataCellStyle).Text(row.Service);
                                table.Cell().Element(DataCellStyle).AlignRight().Text(row.Quantity.ToString("N2"));
                                table.Cell().Element(DataCellStyle).AlignRight().Text(row.Amount.ToString("N2"));
                                table.Cell().Element(DataCellStyle).AlignRight().Text(row.Sum.ToString("N2"));

                                static IContainer DataCellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(10);
                                }
                            }
                        });

                        col.Item().PaddingTop(20);

                        col.Item().AlignRight().Width(200).Column(column =>
                        {
                            column.Item()
                                .Background(Colors.Blue.Medium)
                                .Padding(15)
                                .Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL:").FontSize(14).Bold().FontColor(Colors.White);
                                    row.RelativeItem().AlignRight().Text($"{invoice.TotalSum:N2}").FontSize(16).Bold().FontColor(Colors.White);
                                });
                        });

                        if (!string.IsNullOrWhiteSpace(invoice.Comment))
                        {
                            col.Item().PaddingTop(30);
                            col.Item().Column(column =>
                            {
                                column.Item().Text("NOTES:").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                                column.Item().PaddingTop(5)
                                    .BorderLeft(3)
                                    .BorderColor(Colors.Blue.Medium)
                                    .PaddingLeft(10)
                                    .Text(invoice.Comment)
                                    .FontSize(10)
                                    .Italic();
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).Bold();
                        x.Span(" | Invoice Manager System");
                    });
            });
        });

        return document.GeneratePdf();
    }
}

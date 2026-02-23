using InvoiceMenecerApi.Common;
using AutoMapper;
using InvoiceMenecerApi.Models;
using InvoiceMenecerApi.DTOs.CustomerDto;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceMenecerApi.Services;

public class CustomerService : ICustomerService
{
    private readonly InvoiceMenecerDBContext _context;
    private readonly IMapper _mapper;
    public CustomerService(InvoiceMenecerDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
    {
        var customer = _mapper.Map<Customer>(createCustomerDto);

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();
        await _context
            .Entry(customer)
            .Collection(c => c.Invoices)
            .LoadAsync();

        return _mapper.Map<CustomerResponseDto>(customer);
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync()
    {
        var customer = await _context
            .Customers
            .Include(c => c.Invoices)
            .Where(c => c.DeletedAt == null)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CustomerResponseDto>>(customer);
    }

    public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await _context
            .Customers
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == customerId && c.DeletedAt == null);

        return _mapper.Map<CustomerResponseDto>(customer);
    }

    public async Task<bool> HardDeleteCustomerAsync(Guid customerId)
    {
        var customer = await _context
            .Customers
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == customerId && c.DeletedAt == null);

        if (customer is null) return false;

        var hasSentInvoices = customer.Invoices
            .Any(i => i.Status != InvoiceStatus.Created);

        if (hasSentInvoices) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<bool> ArchiveCustomerAsync(Guid customerId)
    {
        var customer = await _context
            .Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.DeletedAt == null);

        if (customer is null) return false;

        customer.DeletedAt = DateTimeOffset.UtcNow;
        customer.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CustomerResponseDto> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto updateCustomerDto)
    {
        var updatedCustomer = await _context
                                .Customers
                                .Include(c => c.Invoices)
                                .FirstOrDefaultAsync(c => c.Id == customerId && c.DeletedAt == null);

        if (updatedCustomer is null) return null!;

        _mapper.Map(updateCustomerDto, updatedCustomer);
        updatedCustomer.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<CustomerResponseDto>(updatedCustomer);
    }

    public async Task<PagedResult<CustomerResponseDto>> GetAllCustomersPagedAsync(CustomerQueryParams queryParams)
    {
        queryParams.Validate();

        var query = _context
            .Customers
            .Where(c => c.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchTerm = queryParams.Search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm) ||
                (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(searchTerm)) ||
                (c.Address != null && c.Address.ToLower().Contains(searchTerm))
            );
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Name))
        {
            query = query.Where(c => c.Name.ToLower().Contains(queryParams.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Email))
        {
            query = query.Where(c => c.Email.ToLower().Contains(queryParams.Email.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.PhoneNumber))
        {
            query = query.Where(c => c.PhoneNumber != null &&
                c.PhoneNumber.ToLower().Contains(queryParams.PhoneNumber.ToLower()));
        }

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            query = ApplySorting(query, queryParams.Sort, queryParams.SortDirection);
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }

        var skip = (queryParams.Page - 1) * queryParams.Size;
        var customers = await query
            .Skip(skip)
            .Take(queryParams.Size)
            .ToListAsync();

        var customerDtos = _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);

        return PagedResult<CustomerResponseDto>.Create(
            items: customerDtos,
            page: queryParams.Page,
            pageSize: queryParams.Size,
            totalCount: totalCount
        );
    }

    private IQueryable<Customer> ApplySorting(
        IQueryable<Customer> query,
        string sort,
        string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        return sort.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "email" => isDescending
                ? query.OrderByDescending(c => c.Email)
                : query.OrderBy(c => c.Email),
            "createdat" => isDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            "updatedat" => isDescending
                ? query.OrderByDescending(c => c.UpdatedAt)
                : query.OrderBy(c => c.UpdatedAt),
            _ => query.OrderBy(c => c.Name)
        };
    }
}

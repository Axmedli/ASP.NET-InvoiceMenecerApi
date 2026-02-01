using AutoMapper;
using InvoiceMenecer.Models;
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

    public async Task<CustomerResponseDto> GetCustomerByIdAsync(Guid customerId)
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

        if (updatedCustomer is null) return null;

        _mapper.Map(updateCustomerDto, updatedCustomer);
        updatedCustomer.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<CustomerResponseDto>(updatedCustomer);
    }
}
